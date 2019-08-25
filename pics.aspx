<%@ Page Language="VB" AutoEventWireup="false" CodeFile="pics.aspx.vb" Inherits="Pics" %>

<!DOCTYPE html>

<%
    Dim galleryName As String = Request.QueryString("gallery")
    Dim update As String = Request.QueryString("update")
%>

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title><%If galleryName Is Nothing OrElse galleryName.Equals("") Then
                   Response.Write("Select a Gallery...")
               Else
                   Response.Write(galleryName)
               End If
             %></title>
    <!--Import Google Icon Font-->
    <link href="https://fonts.googleapis.com/icon?family=Material+Icons" rel="stylesheet" />
    <!--Import materialize.css-->
    <link type="text/css" rel="stylesheet" href="assets/css/materialize.min.css"  media="screen,projection"/>

    <!--Let browser know website is optimized for mobile-->
    <meta name="viewport" content="width=device-width, initial-scale=1.0"/>
    <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.2.1/jquery.min.js"></script>
    <style type="text/css">
        html{
            background: #F0F0F0;
            font-family: 'Segoe UI','Arial';
            font-size: 12px;
        }
        .container{
            margin: 0 auto;
            padding-left: 5px;
            max-width: 800px;
        }
        .imgThumb{
            height:135px;
            line-height: 135px;
            max-width: 180px;
            background-color: #fafafa;
            border-radius: 5px;
            margin: 4px 4px;
            overflow: hidden;
            display: flex;
            align-items: center;
            justify-content: center;
        }
        .imgThumb:hover{
           background-color: #2196F3;
           cursor: pointer;
            opacity: 0.6;
            -webkit-transition: opacity 0.1s;
            -moz-transition:    opacity 0.1s;
            -o-transition:      opacity 0.1s;
        }
        .imgThumb img{
            max-width: 100%;
            max-height: 100%;
            overflow: hidden;
            vertical-align: middle;
            text-align: center;
            margin: 0 auto;
            padding: 10px 0;
        }
        .selectBox{
            max-width: 200px;
        }
        ul.select-dropdown li span {
            color: #2196F3;
        }
        #galleryFormDiv{
            padding-left: 32px;
        }

        #imageView {
            position: fixed;            
            display: none;
            max-width: 100vw;
            max-height: 100vh;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background-color: rgba(0,0,0,0.5);
            z-index: 2;
        }
        #largeView{
            position: absolute;
            top: 50%;
            left: 50%;
            color: black;
            transform: translate(-50%,-50%);
            -ms-transform: translate(-50%,-50%);
            max-width: 90vw;
            max-height: 90vh;
            z-index: 3;
            border: 8px solid white;
            background-color: white;
            border-radius: 5px;
        }
        #largeView img{
            max-width: 60vw;
            max-height: 60vh;
        }
        #largeView:hover{
            cursor: pointer;
            border-color: #2196F3;
        }
        #data{
            overflow: scroll;
            max-height: 200px;
            padding: 10px;
        }
    </style>
    <script type="text/javascript">
        <%
        Dim pokemonName As String = Request.Form("pokemon")
        %>
        var jsonResponse
        $(document).ready(function () {

            $('select').material_select();

            $(".button-collapse").sideNav();

            if (location.hash.length > 0) {
                var gal = decodeURIComponent(location.hash.slice(1));
                if (typeof (gal) != "undefined") {
                    var query = "app=pics&gallery=" + gal
                    $.post("json.aspx", query,
                        function (data, status) {
                            updateGalleryView(data);
                        });
                }
            }
            else if (document.title === "Select a Gallery...") {
                $("#galleryImages").html("<h3>No gallery selected...</h3>");
            }
            else {
                var query = "app=pics&gallery=" + document.title
                $.post("json.aspx", query,
                    function (data, status) {
                        updateGalleryView(data);
                    });
            }
            $("#submit").click(function () {
                $.post("json.aspx", $('#galleryForm').serialize(),
                    function (data, status) {
                        updateGalleryView(data);
                    });
                $('.button-collapse').sideNav('hide');
            });

        });
        $(document).on('mousedown', '.imgThumb', function (e) {
            if ((e.which == 1)) {
                //alert(this.attributes["data-image"].value)
                getDetails(this.attributes["data-image"].value)
            } else if ((e.which == 2)) {
                window.open((jsonResponse[(this.attributes["data-image"].value.split(",")[0])])['Location'])
            }
            e.preventDefault();
        });
        function getDetails(index) {
            //alert("Real image location here: " + location);
            var location = "./Pics/Edited/" + (jsonResponse[index])['Location'].replace('\\\\','\/')
            var output = "<div id='largeView'><a alt='" + location + "' target='_blank' href='" + location + "'><img  src='" + location + "'></a>"
            output += "<p id='data'><b>Image Name: </b>" + ((jsonResponse[index])['Name']) + "<br/>"
            output += "<b>Gallery Name: </b>" + ((jsonResponse[index])['GalleryName']) + "<br/>"
            output += "<b>Creation Date: </b>" + ((jsonResponse[index])['CreationDate']) + "<br/>"
            output += "<b>Last Modified Date: </b>" + ((jsonResponse[index])['ModifiedDate']) + "<br/>"
            output += "<b>Dimensions: </b>" + ((jsonResponse[index])['Dimensions']) + "px<br/>"
            output += "<b>File Size: </b>" + ((jsonResponse[index])['FileSizeMB']) + "MB<br/>"
            output += "<b>File Extension: </b>" + ((jsonResponse[index])['FileExtension']) + "<br/>"
            output += "<b>MD5 Hash: </b>" + ((jsonResponse[index])['MD5']) + "<br/>"
            output += "<b>URL: </b>" + location + "<br/>"
            output += "<b>Additional Notes: </b><br/>" + ((jsonResponse[index])['Notes']).replace(/(?:\r\n|\r|\n)/g, '<br>') + "</p>"
            output += "<br/></div>"
                document.getElementById("imageView").innerHTML = output
              on("#imageView")          
        }

        function updateGalleryView(json) {
            jsonResponse = json;

            var retVal = "";
            var image = "";
            var temp = "";
            $.each(json, function (index) {
                temp = this['Location'].split("\\");
                var thumbsFolder = "./Pics/Edited_Thumbs/" + temp[0] + "/th_";
                var imageFolder = "./Pics/Edited/" + temp[0] + "/";
                image = temp[1];
                
                retVal = retVal + "<div class='col s6 m4 l3 imgThumb' data-image='" + index + "'><img alt='image'  href ='" + imageFolder + image + "' src='" + thumbsFolder + image + "'/></div>";
                //retVal = retVal + "<div class='col s6 m4 l3 imgThumb'><a href ='" + imageFolder + image + "'><img onclick='getDetails(\"" + index +  "\", \"" + imageFolder + image + "\",event)' alt='image' src='" + thumbsFolder + image + "'/></a></div>";
                //retVal = retVal + "<a href='" + imageFolder + image + "' target='_blank'><div class='col s6 m4 l3 imgThumb'><img class='' alt='image' src='" + thumbsFolder + image + "'/></div></a>";

            });
            document.title = json[0]['GalleryName'];
            location.hash = encodeURIComponent(document.title);
            retVal = "<div class='row'><h3 id='" + document.title + "'>" + document.title + "</h3>" + retVal;
            retVal = retVal + "</div>";
            $("#galleryImages").html(retVal)

            $('.materialboxed').materialbox();
        }
        function on(elem) {
            //document.getElementById(elem).style.display = "block";
            $(elem).fadeIn();
        }

        function off(elem) {
            //document.getElementById(elem).style.display = "none";
            $(elem).fadeOut();
        }
    </script>
    
    <meta charset="utf-8" />

</head>
<body>
    <script type="text/javascript" src="https://code.jquery.com/jquery-3.2.1.min.js"></script>
    <script type="text/javascript" src="assets/js/materialize.min.js"></script>

    <ul id="slide-out" class="side-nav">
        <li><a class="header">Select a Gallery</a></li>
        <li><div class="divider"></div></li>
        <li><div id="galleryFormDiv"><form id="galleryForm">
                <input type="text" style="display:none;" name="app" value="pics" />
                <div class='input-field selectBox blue-text'>
                    <select id="gallery" name="gallery">
                        <%
                            For Each galleryDir As Gallery In Pics.availableGalleries
                                Response.Write("<option value='" + galleryDir.Name + "'>" + galleryDir.Name + "</option>")
                            Next
                        %>
                    </select>
                </div>
                <button id="submit" class="btn waves-effect waves-light blue" type="button">Submit
                    <i class="material-icons right">send</i>
                 </button>
             </form></div>
        </li>
    </ul>
    <a href="#" data-activates="slide-out" class="button-collapse"><i class="material-icons">menu</i></a>
  
    <div class="container">
        <div class="row"></div>
        <div class="row"></div>
        <div class ="row" id="galleryImages">
        </div>
    </div>
  
    <div class ="container"></div>
     <div id="imageView" onclick="off('#imageView')">     </div>        
</body>
</html>
