Partial Class Pics
    Inherits System.Web.UI.Page

    Public Shared workingDir As String = "C:\inetpub\wwwroot\Pics\Edited\"
    Public Shared thumbsDir As String = "C:\inetpub\wwwroot\Pics\Edited_Thumbs\"
    Public Shared workingPublicDir As String = "./Pics/Edited/"
    Public Shared thumbsPublicDir As String = "./Pics/Edited_Thumbs/"

    'currently this list is not repopulated unless you build the site again
    Public Shared Property availableGalleries As ArrayList = DirsGet()

    Shared Function DirsGet() As ArrayList

        Dim myGalleries As ArrayList = New ArrayList()

        'GetAllGalleriesQuery returns the ID (0) and Name (1) from the query
        For Each foundDirectory In GetAllGalleriesQuery()
            Dim newGallery As Gallery = New Gallery(foundDirectory, workingDir)
            myGalleries.Add(newGallery)
        Next
        Return myGalleries

    End Function

    'GetAllGalleriesQuery returns the ID (0) and Name (1) from the query
    'TO DO: make it pull the modified date etc
    Shared Function GetAllGalleriesQuery() As ArrayList
        Dim commandText As String = ("SELECT * FROM [Pics].[dbo].[Gallery] with (nolock) Order By [Name] Asc")

        Dim retval As ArrayList = Common.RunQuery(commandText, Nothing)

        Return retval
    End Function

    'TO DO: make it query for select galleries only
    Shared Function GetAllImagesQuery(params As String) As ArrayList
        Dim commandText As String = ("SELECT * FROM [Pics].[dbo].[Image] with (nolock)")

        Dim retval As ArrayList = Common.RunQuery(commandText, Nothing)

        Return retval
    End Function

    'Returns List of Gallery Objects based on list of Gallery Names desired
    Shared Function GetByGalleryName(galleryNames As ArrayList) As ArrayList
        Dim retVal As New ArrayList()
        For Each galleryName In galleryNames
            For Each tempGallery In availableGalleries
                If tempGallery.Name.Equals(galleryName) Then
                    retVal.Add(tempGallery)
                    Exit For
                End If
            Next
        Next
        Return retVal

    End Function

End Class


