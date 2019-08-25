<%@ Page Language="VB" AutoEventWireup="false" CodeFile="Manage.aspx.vb" Inherits="Manage" %>
<%=User.Identity.Name%>
<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Manage the Galleries</title>
    <!--Import Google Icon Font-->
    <link href="https://fonts.googleapis.com/icon?family=Material+Icons" rel="stylesheet" />
    <!--Import materialize.css-->
    <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.2.1/jquery.min.js"></script>
    <style>
        table{
            border: 1px solid black;
            border-collapse: collapse;
            max-width: 1000px;
            min-width: 500px;
        }
        .head{
            display: table-row !important;
        }
        .directory {
            display: table-row !important;
        }
        .directory td{
            text-align: left;
            font-weight: bold;
        }
        .directory:hover{
           background-color: #2196F3;
           cursor: pointer;
        }
        tr{
            display: none;
        }
        th{
            text-align: left;
            font-size: 20px;
            border: 1px solid black;
            background-color: black;
            color: white;
        }
        td{
            text-align: center;
            border: 1px solid black;
        }
    </style>
    <script type='text/javascript'>
        function toggleMyClass(myclass) {
            var $tmp = myclass.innerText
            $("." + $tmp).each(function () { 
                if($(this).css("display") != "none") {
                    $(this).css("display","none")
                }
                else {
                    $(this).css("display", "table-row")
                }
            });


        }
    </script>
</head>
<body>
    <div>
    <%

        Dim isUpdate As String = GetQuery("update")
        Dim galleryName As String = GetQuery("gallery")
        Dim method As String = GetQuery("method")
        If isUpdate IsNot "" AndAlso isUpdate IsNot Nothing Then
            If galleryName IsNot "" AndAlso galleryName IsNot Nothing Then
                If method.Equals("folder") Then
                    Response.Write(UpdateGalleryByFolder(galleryName))
                Else
                    Response.Write("To-Do.")
                End If
            Else
                If isUpdate.Equals("all") Then
                    Response.Write(directoryForm())
                End If
            End If
        Else
            Response.Write("<h2>Wouldn't you like to know...</h2>")
        End If
        %>
    </div>
</body>
</html>
