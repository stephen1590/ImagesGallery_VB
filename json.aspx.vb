
Imports System.Data
Imports System.Data.SqlClient
Imports Newtonsoft.Json

Partial Class json
    Inherits System.Web.UI.Page

    Public Function GetGallery(galleryName As String) As String
        Dim commandText As String = "SELECT *"
        Dim paramArrayList As New ArrayList
        If (galleryName <> "*" AndAlso galleryName <> "") Then
            commandText += ", '" + galleryName + "' as 'GalleryName' FROM [Pics].[dbo].[Image] with (nolock) where ID IN (SELECT ImageID FROM [Pics].[dbo].[GalleryImageList] with (nolock) Where GalleryID = (Select ID From [Pics].[dbo].[Gallery] with (nolock) where [Name] = @galleryName)) "
            Dim param As SqlParameter = New SqlParameter("@galleryName", SqlDbType.VarChar, 50)
            param.Value = galleryName
            paramArrayList.Add(param)
        Else
            commandText += ", 'Multiple Galleries Selected' as 'GalleryName' FROM [Pics].[dbo].[Image] with (nolock) "
        End If
        commandText += "Order By [Location] Asc"
        Dim retval As ArrayList = Common.RunQuery(commandText, paramArrayList)
        Dim jsonString As String = JsonConvert.SerializeObject(retval)

        Return jsonString
    End Function

    Public Function GetQuery(application As String) As String
        Dim var As String = ""
        'Response.Write(application)
        Dim varForm = Request.Form(application)
        Dim varQuery = Request.QueryString(application)


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
