
Imports System.IO

Imports System.Drawing
Imports System.Data.SqlClient
Imports System.Data
Imports Newtonsoft.Json

Partial Class Manage
    Inherits System.Web.UI.Page

    Public Shared workingDir As String = "C:\inetpub\wwwroot\Pics\Edited\"
    Public Shared thumbsDir As String = "C:\inetpub\wwwroot\Pics\Edited_Thumbs\"
    Public Shared workingPublicDir As String = "./Pics/Edited/"
    Public Shared thumbsPublicDir As String = "./Pics/Edited_Thumbs/"

    Shared Function getDirectories() As DirectoryInfo()
        Dim di As DirectoryInfo = New DirectoryInfo(workingDir)
        Dim retval As DirectoryInfo() = di.GetDirectories()
        Return retval
    End Function

    Function UpdateGalleryByFolder(galName As String) As String
        Dim fileInfo As FileInfo
        Dim thumbsFileInfo As FileInfo

        Dim retVal = ""

        'Add the Gallery to the DB if it does not exist already (unique names)
        Dim galID As Integer = CreateGalleryInDB(galName)

        'Check if we have existing images in the gallery & cross reference with what is currently in the images DB
        Dim existingGalleryImages As ArrayList = GetExistingGalleryImages(galID)

        Dim imagesToAdd As ArrayList = New ArrayList()

        For Each foundImgFile In My.Computer.FileSystem.GetFiles(workingDir + galName)
            fileInfo = My.Computer.FileSystem.GetFileInfo(foundImgFile)
            If (fileInfo.Extension.ToLower = ".jpg") Or (fileInfo.Extension.ToLower = ".jpeg") Or (fileInfo.Extension.ToLower = ".png") Then
                Dim temp As String = Common.CreateMD5StringFromFile(foundImgFile)
                If Not existingGalleryImages.Contains(temp) Then
                    Dim tempList As New ArrayList()
                    tempList.Insert(0, temp)
                    tempList.Insert(1, foundImgFile)
                    tempList.Insert(2, fileInfo.Name)
                    imagesToAdd.Add(tempList)
                End If
            End If

        Next

        If imagesToAdd.Count > 0 Then
            retVal += AddImagesToDB(imagesToAdd, galName, galID)
        End If

        'Now we create the thumbs!
        Dim thumbsFolder As String = thumbsDir + galName

        If (Not System.IO.Directory.Exists(thumbsFolder)) Then
            System.IO.Directory.CreateDirectory(thumbsFolder)
        End If

        Dim saveImage As Image
        Dim tempImage As Image

        For Each img As ArrayList In imagesToAdd
            thumbsFileInfo = My.Computer.FileSystem.GetFileInfo(thumbsFolder + "\th_" + img(2))
            If Not thumbsFileInfo.Exists Then
                'Generate new thumb
                tempImage = Image.FromFile(img(1))
                saveImage = ScaleImage(tempImage, 200, 200)
                tempImage.Dispose()
                saveImage.Save(thumbsFileInfo.FullName)
                saveImage.Dispose()
            End If
        Next

        Return retVal
    End Function
    Function directoryForm() As String
        Dim retVal = ""
        retVal += "<table align='center'>"
        retVal += "<tr class='head'><th>Filename</th><th>Size (MB)</th><th>Last Modified</th><th>CheckBox</th></tr>"
        Dim dirname As String = ""
        For Each Dir As System.IO.DirectoryInfo In getDirectories()
            dirname = (Dir.Name).Replace(" ", "").Replace("-", "")
            retVal += "<tr class='directory' onClick='toggleMyClass(" + dirname + ")'><td id='" + dirname + "' colspan='4'>" + dirname + "</td></tr>"
            For Each fileInDir As System.IO.FileInfo In Dir.GetFiles()
                retVal += "<tr class='" + dirname + "'><td>" + fileInDir.Name + " <a href='./Pics/Edited/" + Dir.Name + "/" + fileInDir.Name + "'>(link)</a></td><td>" + Math.Round((fileInDir.Length / 1024 / 1024), 2).ToString() + "</td><td>" + fileInDir.LastWriteTime + "</td><td> <input type='checkbox' name='" + fileInDir.Name + "' value='" + fileInDir.Name + "'></td></tr>"
            Next
        Next
        retVal += "</table>"
        Return retVal
    End Function
    'There is a much better way to do this other than loop through and do a bunch of queries, but I'm too lazy right now
    Function AddImagesToDB(data As ArrayList, galName As String, galID As Integer) As String

        Dim jsonString As String = ""
        Dim fileInfo As FileInfo

        For Each image In data

            Dim commandText As String = "INSERT INTO [Pics].[dbo].[Image]([MD5],[Location],[OwnerID],[ModifiedDate])VALUES( @MD5 , @location,-1,@ModifiedDate)" + Environment.NewLine + "Select [ID] From [Pics].[dbo].[Image] with (nolock) where [MD5] = @MD5"

            Dim paramArrayList As New ArrayList()


            Dim MD5 As String = image(0)
            fileInfo = My.Computer.FileSystem.GetFileInfo(image(1))
            Dim filename As String = galName + "\" + fileInfo.Name

            Dim sqlParam As SqlParameter = New SqlParameter("@MD5", SqlDbType.VarChar, 50)
            sqlParam.Value = MD5
            paramArrayList.Add(sqlParam)

            sqlParam = New SqlParameter("@location", SqlDbType.NVarChar, 300)
            sqlParam.Value = filename
            paramArrayList.Add(sqlParam)

            sqlParam = New SqlParameter("@ModifiedDate", SqlDbType.DateTime)
            sqlParam.Value = fileInfo.CreationTime
            paramArrayList.Add(sqlParam)

            Dim retval As ArrayList = Common.RunQuery(commandText, paramArrayList)
            jsonString += JsonConvert.SerializeObject(retval)

            'now add the images to the gallery - dynamic stuff coming later
            If retval IsNot Nothing Then
                commandText = "INSERT INTO [Pics].[dbo].[GalleryImageList]([GalleryID],[ImageID])VALUES(@galID,@imgID)"

                paramArrayList = New ArrayList()

                sqlParam = New SqlParameter("@galID", SqlDbType.Int)
                sqlParam.Value = galID
                paramArrayList.Add(sqlParam)

                sqlParam = New SqlParameter("@imgID", SqlDbType.Int)
                sqlParam.Value = retval(0)("ID")
                paramArrayList.Add(sqlParam)

                retval = Common.RunQuery(commandText, paramArrayList)
            Else
                Throw New Exception("Error Inserting to DB.")
            End If


        Next


        Return jsonString
    End Function

    Function GetExistingGalleryImages(galID As Integer) As ArrayList
        Dim commandText As String = "SELECT [MD5] FROM [Pics].[dbo].[Image] with (nolock) where [ID] IN (SELECT [ImageID] FROM [Pics].[dbo].[GalleryImageList] with (nolock) Where GalleryID = @galID)"
        Dim paramArrayList As New ArrayList

        Dim param As SqlParameter = New SqlParameter("@galID", SqlDbType.Int)

        param.Value = galID
        paramArrayList.Add(param)

        Dim temp As ArrayList = Common.RunQuery(commandText, paramArrayList)

        Dim retval As ArrayList = New ArrayList()
        For Each md5 In temp
            retval.Add(md5("MD5"))
        Next

        Return retval
    End Function

    Function CreateGalleryInDB(galName As String) As Integer
        Dim retVal As Integer = DoesGalleryAlreadyExist(galName)
        If retVal.Equals(-1) Then
            Dim commandText As String = ("INSERT INTO [Pics].[dbo].[Gallery] ([Name],[OwnerID],[CreatedDate],[ModifiedDate])VALUES(@name, NULL, @CreatedDate, @ModifiedDate)" + Environment.NewLine + "Select * From [Pics].[dbo].[Gallery]  with (nolock) where [Name] = @name")
            Dim paramArrayList As New ArrayList

            Dim sqlParam As SqlParameter = New SqlParameter("@name", SqlDbType.VarChar, 50)
            sqlParam.Value = galName
            paramArrayList.Add(sqlParam)

            sqlParam = New SqlParameter("@CreatedDate", SqlDbType.DateTime)
            sqlParam.Value = DateTime.Now()
            paramArrayList.Add(sqlParam)

            sqlParam = New SqlParameter("@ModifiedDate", SqlDbType.DateTime)
            sqlParam.Value = DateTime.Now()
            paramArrayList.Add(sqlParam)

            Dim resultsString As ArrayList = Common.RunQuery(commandText, paramArrayList)

            If resultsString(0) IsNot Nothing Then
                retVal = resultsString(0)("ID")
            End If

            Return retVal
        End If

        Return retVal

    End Function

    Function DoesGalleryAlreadyExist(galName As String) As Integer
        Dim retVal As Integer = -1
        Dim paramArrayList As New ArrayList

        Dim commandText As String = ("SELECT [ID] FROM [Pics].[dbo].[Gallery] with (nolock) where [Name] = @name")
        Dim sqlParam As SqlParameter = New SqlParameter("@name", SqlDbType.VarChar, 50)
        sqlParam.Value = galName
        paramArrayList.Add(sqlParam)

        Dim queryResponse As ArrayList = Common.RunQuery(commandText, paramArrayList)
        If queryResponse.Count > 0 Then
            retVal = queryResponse(0)("ID")
        End If
        Return retVal
    End Function

    Shared Function ScaleImage(ByVal OldImage As Image, ByVal TargetHeight As Integer, ByVal TargetWidth As Integer) As Image

        Dim NewHeight As Integer = TargetHeight
        Dim NewWidth As Integer = NewHeight / OldImage.Height * OldImage.Width

        If NewWidth > TargetWidth Then
            NewWidth = TargetWidth
            NewHeight = NewWidth / OldImage.Width * OldImage.Height
        End If

        Return New Bitmap(OldImage, NewWidth, NewHeight)

    End Function

    Public Function GetQuery(query As String) As String
        Dim var As String = ""
        'Response.Write(application)
        Dim varForm = Request.Form(query)
        Dim varQuery = Request.QueryString(query)


        If varForm IsNot Nothing Then
            'For Debugging
            'Response.Write(varForm)
            var = varForm
        ElseIf varQuery IsNot Nothing Then
            'For Debugging
            'Response.Write(varQuery)
            var = varQuery
        End If

        Return var

    End Function

End Class
