<%@ Page Title="Sign Document"
    Language="C#"
    MasterPageFile="~/Site.Master"
    AutoEventWireup="true"
    CodeBehind="SignDocument.aspx.cs"
    Inherits="DigitalSignatureProject.SignDocument" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <style>
        .page-box {
            background: #ffffff;
            padding: 20px;
            border-radius: 8px;
            box-shadow: 0 0 10px rgba(0,0,0,0.1);
        }

        .title {
            font-size: 22px;
            font-weight: 600;
            margin-bottom: 20px;
        }

        .row {
            margin-bottom: 15px;
        }

        .label {
            font-weight: 600;
            display: block;
            margin-bottom: 5px;
        }

        .dropdown {
            width: 250px;
            padding: 6px;
        }

        .pdf-area {
            height: 450px;
            border: 2px dashed #999;
            display: flex;
            align-items: center;
            justify-content: center;
            color: #666;
            margin-top: 15px;
        }

        .btn-next {
            background-color: #007bff;
            color: white;
            border: none;
            padding: 8px 18px;
            border-radius: 5px;
            cursor: pointer;
        }

        .btn-next:hover {
            background-color: #0056b3;
        }
    </style>

    <div class="page-box">

        <div class="title">Sign Document</div>

        <div class="row">
            <span class="label">Select Organization</span>
            <asp:DropDownList ID="ddlOrg" runat="server" CssClass="dropdown">
                <asp:ListItem Text="ZMCL" />
                <asp:ListItem Text="ISPL" />
                <asp:ListItem Text="ZOSES" />
            </asp:DropDownList>
        </div>

        <div class="row">
            <span class="label">Select Vendor</span>
            <asp:DropDownList ID="ddlVendor" runat="server" CssClass="dropdown">
                <asp:ListItem Text="Vendor 1" />
                <asp:ListItem Text="Vendor 2" />
                <asp:ListItem Text="Vendor 3" />
            </asp:DropDownList>
        </div>

        <div class="pdf-area">
            PDF Preview Area (Sign locations will appear here)
        </div>

        <br />

        <div class="row">
            <span class="label">Select Pages to Sign</span>
            <asp:CheckBoxList ID="cblPages" runat="server" CssClass="dropdown" RepeatDirection="Horizontal" />
        </div>

        <asp:Button ID="btnNext"
            runat="server"
            Text="Next"
            CssClass="btn-next"
            OnClick="btnNext_Click" />

    </div>

</asp:Content>
