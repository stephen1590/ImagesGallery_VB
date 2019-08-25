<%@ Page Language="VB" AutoEventWireup="false" CodeFile="json.aspx.vb" Inherits="json" %>
<%
    Response.ContentType = "application/json"
    Dim application = GetQuery("app")

    Dim collection As NameValueCollection = Request.Form

    'For Each formResponse As String In Request.Form
    '    Response.Write(formResponse)
    'Next

    If application IsNot Nothing Then

        If application.ToLower() = "pics" Then
            Dim galleryName As String = GetQuery("gallery")
            If galleryName <> "" Then
                Response.Write(GetGallery(galleryName))
            End If
        End If

    End If

%>