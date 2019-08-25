Imports System.IO

Imports System.Drawing

'-------------------------------------------------------------------------------------------------------------------------
'
'--------------------------------Gallery Image Class
'
'-------------------------------------------------------------------------------------------------------------------------

Public Class GalleryImage

    Private ImageID As Int16
    Private ImageName As String = ""
    Private ImageLocation As String = ""
    Private ImageModifiedDate As DateTime
    Private ImageCreateDate As DateTime
    Private md5 As String = ""

    Property ID As Int16
        Get
            Return ImageID
        End Get
        Set(value As Int16)
            ImageID = value
        End Set
    End Property

    Property Name As String
        Get
            Return ImageName
        End Get
        Set(value As String)
            ImageName = value
        End Set
    End Property

    Property FileLocation As String
        Get
            Return ImageLocation
        End Get
        Set(value As String)
            ImageLocation = value
        End Set
    End Property

    Property ModifiedDate As DateTime
        Get
            Return ImageModifiedDate
        End Get
        Set(value As DateTime)
            ImageModifiedDate = value
        End Set
    End Property

    Property CreateDate As DateTime
        Get
            Return ImageCreateDate
        End Get
        Set(value As DateTime)
            ImageCreateDate = value
        End Set
    End Property

    Property Hash As String
        Get
            Return md5
        End Get

        'This value will be a filename
        Set(value As String)
            md5 = Common.CreateMD5StringFromFile(value)
        End Set

    End Property

    Public Sub New()

    End Sub

    Public Sub New(imageArray As ArrayList)

        ImageID = imageArray(0)
        md5 = imageArray(1)
        Dim location As Array = imageArray(2).Split("\")
        ImageLocation = location(0)
        ImageName = location(1)
    End Sub

    Public Sub New(iID As Int16, iName As String, iLocation As String)

        ImageID = iID
        ImageName = iName
        ImageLocation = iLocation
        md5 = Common.CreateMD5StringFromFile(ImageLocation)

        Dim fs As New FileInfo(ImageLocation)
        ImageModifiedDate = fs.LastWriteTimeUtc
        ImageCreateDate = fs.CreationTimeUtc

    End Sub

End Class

'-------------------------------------------------------------------------------------------------------------------------
'
'--------------------------------Gallery Class
'
'-------------------------------------------------------------------------------------------------------------------------

Public Class Gallery
    
    Public Shared workingDir As String = "C:\inetpub\wwwroot\Pics\Edited\"
    Public Shared thumbsDir As String = "C:\inetpub\wwwroot\Pics\Edited_Thumbs\"
    Public Shared workingPublicDir As String = "./Pics/Edited/"
    Public Shared thumbsPublicDir As String = "./Pics/Edited_Thumbs/"

    Public Shared Property fileStats As FileInfo
    Public Shared Property fileWriter As Object

    Private GalleryName As String = ""
    Private GalleryID As Int16
    Private GalleryImages As ArrayList = New ArrayList()
    Private GalleryDir As String = ""
    Private GalleryModified As DateTime

    Property ID As Int16
        Get
            Return GalleryID
        End Get
        Set(value As Int16)
            GalleryID = value
        End Set
    End Property

    Property Name As String
        Get
            Return GalleryName
        End Get
        Set(value As String)
            GalleryName = value
        End Set
    End Property

    Property Images As ArrayList
        Get
            Return GalleryImages
        End Get
        Set(value As ArrayList)
            GalleryImages = populateGalleryImages()
        End Set
    End Property

    Property Location As String
        Get
            Return GalleryDir
        End Get
        Set(value As String)
            GalleryDir = value
        End Set
    End Property

    Property ModifiedDate As DateTime
        Get
            Return GalleryModified
        End Get
        Set(value As DateTime)
            GalleryModified = value
        End Set
    End Property

    Public Sub New()

    End Sub

    Public Sub New(galleryArray As HashTable, workingDir As String)
        GalleryID = galleryArray("ID")
        GalleryName = galleryArray("Name")
        GalleryDir = workingDir + galleryArray("Location")
        'don't populate the gallery Images just yet!
        'GalleryImages = populateGalleryImages()
    End Sub

    Public Sub New(gID As Int16, gName As String, gDir As String)
        GalleryID = gID
        GalleryName = gName
        GalleryDir = gDir
        GalleryImages = populateGalleryImages()
        'GenerateThumbnails(me)
    End Sub

    Private Function populateGalleryImages() As ArrayList

        ' Dim fileStats As FileInfo
        Dim imageFolder As String = ""
        Dim imageName = ""
        Dim img As GalleryImage

        Dim imageArray As New ArrayList()

        Dim commandText As String = "SELECT * FROM [Pics].[dbo].[Image] with (nolock) where ID IN (SELECT ImageID FROM [Pics].[dbo].[GalleryImageList] with (nolock) Where GalleryID = " + GalleryID.ToString() + ")"

        Dim dbItem As ArrayList = Common.RunQuery(commandText, Nothing)

        For Each entry In dbItem
            img = New GalleryImage(entry)
            imageArray.Add(img)
        Next

        Return imageArray

    End Function

        Shared Function GenerateThumbnails(gal As Gallery) As Boolean

        Dim thumbsFolder As String = thumbsDir + gal.Name
        Dim fileInfo As FileInfo
        Dim thumbsFileInfo As FileInfo
        Dim saveImage As Image
        Dim tempImage As Image

        If (Not System.IO.Directory.Exists(thumbsFolder)) Then
            System.IO.Directory.CreateDirectory(thumbsDir + gal.Name)
        End If

        For Each foundImgFile In My.Computer.FileSystem.GetFiles(gal.Location)
            fileInfo = My.Computer.FileSystem.GetFileInfo(foundImgFile)
            If (fileInfo.Extension.ToLower = ".jpg") Or (fileInfo.Extension.ToLower = ".jpeg") Or (fileInfo.Extension.ToLower = ".png") Then
                thumbsFileInfo = My.Computer.FileSystem.GetFileInfo(thumbsFolder + "\th_" + fileInfo.Name)
                If Not thumbsFileInfo.Exists Then
                    'Generate new thumb
                    tempImage = Image.FromFile(fileInfo.FullName)
                    saveImage = ScaleImage(tempImage, 200, 200)
                    tempImage.Dispose()
                    saveImage.Save(thumbsFileInfo.FullName)
                    saveImage.Dispose()
                End If
            End If
        Next

        Return True
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

End Class