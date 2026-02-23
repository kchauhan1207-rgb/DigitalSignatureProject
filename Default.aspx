<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="DigitalSignatureProject._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

   <!-- Add Bootstrap CSS in head -->
<link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css" rel="stylesheet">

<div class="container mt-4" style="background-color:#ffffff; padding:20px; border-radius:10px; 
     box-shadow:0 4px 8px rgba(0,0,0,0.1); max-width:700px; margin:50px auto;">

    <h2 class="mb-3">Upload PDF for Digital Signature</h2>

    <!-- File Upload -->
    <div class="mb-3">
        <asp:FileUpload ID="FileUpload1" runat="server" CssClass="form-control" />
    </div>

    <!-- Upload Button -->
    <a href="About.aspx">About.aspx</a>
   <asp:Button ID="btnUpload" runat="server" Text="Upload PDF" CssClass="btn btn-primary w-100"
    OnClientClick="document.getElementById('loadingSpinner').style.display='block';" 
    OnClick="btnUpload_Click" />

     <!-- spinner (initially hidden)-->
     <div id="loadingSpinner" style="display: none; margin-top:10px;">
     <img src="images/spinner.gif" width="50" />
    <p style="color:#555;">uploading....</p>
     </div>

    <!-- Message -->
    <asp:Label ID="lblMessage" runat="server" CssClass="mt-2 d-block text-center" style="font-weight:bold;"></asp:Label>

    <!-- PDF Preview -->
    <div class="mt-3">
        <iframe id="pdfPreview" runat="server" width="100" height="500" style="border:1px solid #ccc; border-radius:8px; box-shadow: 0 2px 6px rgba(0,0,0,0.1); margin-top:20px;"></iframe>
    </div>
</div>

</asp:Content>