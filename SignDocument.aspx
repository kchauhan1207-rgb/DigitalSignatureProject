<%@ Page Title="Sign Document"
    Language="C#"
    MasterPageFile="~/Site.Master"
    AutoEventWireup="true"
    CodeBehind="SignDocument.aspx.cs"
    Inherits="DigitalSignatureProject.SignDocument" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

<style>
    .page-box { background:#fff; padding:20px; border-radius:8px; }
    .title { font-size:22px; font-weight:600; margin-bottom:20px; }
    .row { margin-bottom:15px; }
    .label { font-weight:600; display:block; margin-bottom:5px; }
    .dropdown { width:250px; padding:6px; }
    .pdf-area { height:450px; border:2px dashed #999; position:relative; margin-top:15px; }
    .overlay-tip { position:absolute; right:10px; top:10px; background:#fff; padding:6px 10px; border-radius:6px; }
    .sig-marker { position:absolute; width:18px; height:18px; background:#dc3545; color:#fff; border-radius:50%;
                  transform:translate(-50%,-50%); display:flex; align-items:center; justify-content:center; font-size:11px; }
</style>

<div class="page-box">
    <div class="title">Sign Document</div>

    <div class="row">
        <span class="label">Select Organization</span>
        <asp:DropDownList ID="ddlOrg" runat="server" CssClass="dropdown"
            AutoPostBack="true" OnSelectedIndexChanged="ddlOrg_SelectedIndexChanged">
            <asp:ListItem Text="ZMCL" Value="ZMCL" />
            <asp:ListItem Text="ISPL" Value="ISPL" />
            <asp:ListItem Text="ZOSES" Value="ZOSES" />
        </asp:DropDownList>
    </div>

    <div class="row">
        <span class="label">Select Vendor</span>
        <asp:DropDownList ID="ddlVendor" runat="server" CssClass="dropdown"
            AutoPostBack="true" OnSelectedIndexChanged="ddlVendor_SelectedIndexChanged">
            <asp:ListItem Text="Vendor 1" Value="Vendor1" />
            <asp:ListItem Text="Vendor 2" Value="Vendor2" />
        </asp:DropDownList>
    </div>

    <div class="row">
        <span class="label">Select Location</span>
        <asp:DropDownList ID="ddlLocation" runat="server" CssClass="dropdown" />
        <asp:Literal ID="litLocationScript" runat="server" />
    </div>

    <asp:Literal ID="litMessage" runat="server" />
    <asp:Literal ID="litSignedScript" runat="server" />

    <div class="pdf-area" id="pdfContainer">
        <div class="overlay-tip">Click on PDF to place signature</div>

        <%-- SERVER SAFE COMMENT (IMPORTANT) --%>
        <div id="pdfPreviewContainer" runat="server" style="width:100%; height:100%;">
            <iframe id="pdfFrame" width="100%" height="100%" style="border:none;"></iframe>
        </div>

        <div id="markerLayer"
             style="position:absolute; left:0; top:0; width:100%; height:100%; pointer-events:none;">
        </div>
    </div>

    <div class="row">
        <span class="label">Select Pages</span>
        <asp:CheckBoxList ID="cblPages" runat="server" RepeatDirection="Horizontal"
            AutoPostBack="true" OnSelectedIndexChanged="cblPages_SelectedIndexChanged" />
    </div>

    <asp:HiddenField ID="hfSelectedPage" runat="server" />
    <asp:HiddenField ID="hfClickX" runat="server" />
    <asp:HiddenField ID="hfClickY" runat="server" />

    <asp:Button ID="btnConfirmSign" runat="server" Text="Confirm Signature"
        OnClick="btnConfirmSign_Click" Style="display:none" />

<script type="text/javascript">
    (function () {
        // Server-side: resolve file name (prefer preview), get only file name (not full path)
        var uploadsBase = '<%= ResolveUrl("~/Uploads/") %>';
        var pdfFileName = '<%= System.IO.Path.GetFileName((string)Session["PreviewPDF"] ?? (string)Session["PDFPath"] ?? "") %>';

        function firstChecked() {
            var boxes = document.querySelectorAll('#<%= cblPages.ClientID %> input[type=checkbox]');
            for (var i = 0; i < boxes.length; i++) {
                if (boxes[i].checked) return boxes[i].value;
            }
            return null;
        }

        window.showSelectedPage = function () {
            if (!pdfFileName) {
                alert('PDF not found on server. Please upload first.');
                return;
            }
            var p = firstChecked();
            if (!p) return;
            document.getElementById('pdfFrame').src = uploadsBase + pdfFileName + '#page=' + p;
        };

        // Defer attaching the click handler until DOM is ready
        function attachHandlers() {
            var container = document.getElementById('pdfContainer');
            if (!container) return;

            container.addEventListener('click', function (e) {
                var iframe = document.getElementById('pdfFrame');
                var r = iframe.getBoundingClientRect();

                // If iframe not visible or not loaded, abort
                if (r.width === 0 || r.height === 0) return;

                var xPercent = ((e.clientX - r.left) / r.width * 100).toFixed(2);
                var yPercent = ((e.clientY - r.top) / r.height * 100).toFixed(2);

                document.getElementById('<%= hfClickX.ClientID %>').value = xPercent;
                document.getElementById('<%= hfClickY.ClientID %>').value = yPercent;
                document.getElementById('<%= hfSelectedPage.ClientID %>').value = firstChecked();
                document.getElementById('<%= btnConfirmSign.ClientID %>').style.display = 'inline-block';
            });

            // Render page when a checkbox changes on client side (no full postback required)
            document.querySelector('#<%= cblPages.ClientID %>').addEventListener('change', function () {
                // showSelectedPage will no-op if no checked box
                window.showSelectedPage();
                // also attempt to render markers if server emitted signedMarkers
                if (typeof renderMarkersForPage === 'function') {
                    var p = firstChecked();
                    if (p) renderMarkersForPage(parseInt(p, 10));
                }
            });
        }

        // Small helper to render markers if litSignedScript produced signedMarkers
        window.renderMarkersForPage = window.renderMarkersForPage || function (page) {
            var layer = document.getElementById('markerLayer');
            if (!layer) return;
            layer.innerHTML = '';
            if (typeof signedMarkers === 'undefined' || !Array.isArray(signedMarkers)) return;
            signedMarkers.forEach(function (m, idx) {
                if (!m || parseInt(m.Page, 10) !== page) return;
                var x = parseFloat(m.XPercent) || 0;
                var y = parseFloat(m.YPercent) || 0;
                var el = document.createElement('div');
                el.className = 'sig-marker';
                el.style.left = x + '%';
                el.style.top = y + '%';
                el.title = (m.Organization || '') + ' / ' + (m.Vendor || '') + ' / ' + (m.Location || '');
                el.innerText = (idx + 1);
                layer.appendChild(el);
            });
        };

        // Initialize on DOM ready
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', function () {
                attachHandlers();
                // show first selected page automatically
                setTimeout(window.showSelectedPage, 150);
            });
        } else {
            attachHandlers();
            setTimeout(window.showSelectedPage, 150);
        }
    })();
</script>

</div>
</asp:Content>