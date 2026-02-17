<%@ Page Title="Selected Pages"
    Language="C#"
    MasterPageFile="~/Site.Master"
    AutoEventWireup="true"
    CodeBehind="NextPage.aspx.cs"
    Inherits="DigitalSignatureProject.NextPage" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        .page-box { background:#fff; padding:20px; border-radius:8px; box-shadow:0 0 10px rgba(0,0,0,0.1); }
        .title { font-size:22px; font-weight:600; margin-bottom:12px; }
        .links { margin-bottom:12px; }
    </style>

    <div class="page-box">
        <div class="title">Selected Pages</div>
        <asp:Literal ID="litMessage" runat="server" />
        <div class="links" id="pagesList" runat="server"></div>
        <iframe id="pdfFrame" runat="server" style="width:100%; height:600px; border:1px solid #ccc;"></iframe>
    </div>
</asp:Content>