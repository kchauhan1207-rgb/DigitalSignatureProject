<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ApplySignature.aspx.cs" 
         Inherits="DigitalSignatureProject.ApplySignature" %>

<!DOCTYPE html>
<html>
<head>
    <title>Apply Digital Signatures - Module 3</title>
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
            padding: 20px;
        }
        .container {
            max-width: 900px;
            margin: 0 auto;
            background: white;
            padding: 40px;
            border-radius: 15px;
            box-shadow: 0 10px 40px rgba(0,0,0,0.2);
        }
        .header {
            text-align: center;
            margin-bottom: 30px;
            padding-bottom: 20px;
            border-bottom: 3px solid #667eea;
        }
        .header h1 { color: #333; font-size: 32px; margin-bottom: 10px; }
        .header p { color: #666; }
        .section {
            background: #f8f9fa;
            padding: 20px;
            border-radius: 10px;
            margin-bottom: 20px;
        }
        .section h3 { color: #667eea; margin-bottom: 15px; }
        .info-row {
            display: flex;
            justify-content: space-between;
            padding: 10px 0;
            border-bottom: 1px solid #e0e0e0;
        }
        .info-row:last-child { border-bottom: none; }
        .label { font-weight: 600; color: #555; }
        .value { color: #333; }
        .signature-item {
            padding: 12px;
            background: #e8f5e9;
            border-left: 4px solid #4caf50;
            margin-bottom: 10px;
            border-radius: 5px;
        }
        .btn {
            padding: 15px 40px;
            border: none;
            border-radius: 8px;
            font-size: 16px;
            font-weight: 600;
            cursor: pointer;
            margin: 10px 5px;
            transition: all 0.3s;
        }
        .btn:hover { transform: translateY(-2px); }
        .btn-apply {
            background: linear-gradient(135deg, #4caf50 0%, #45a049 100%);
            color: white;
        }
        .btn-download {
            background: linear-gradient(135deg, #2196f3 0%, #1976d2 100%);
            color: white;
        }
        .success-panel {
            background: linear-gradient(135deg, #e8f5e9 0%, #c8e6c9 100%);
            padding: 25px;
            border-radius: 10px;
            border-left: 5px solid #4caf50;
            margin-top: 20px;
            display: none;
        }
        .success-panel h3 { color: #2e7d32; margin-bottom: 15px; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="container">
            <div class="header">
                <h1>✍️ Apply Digital Signatures</h1>
                <p>Module 3 - Janmesh (Digital Signature Application & Save)</p>
            </div>

            <div class="section">
                <h3>📄 Document Information</h3>
                <div class="info-row">
                    <span class="label">Document ID:</span>
                    <span class="value"><asp:Label ID="lblDocId" runat="server"></asp:Label></span>
                </div>
                <div class="info-row">
                    <span class="label">File Name:</span>
                    <span class="value"><asp:Label ID="lblFileName" runat="server"></asp:Label></span>
                </div>
                <div class="info-row">
                    <span class="label">Total Pages:</span>
                    <span class="value"><asp:Label ID="lblPages" runat="server"></asp:Label></span>
                </div>
                <div class="info-row">
                    <span class="label">Status:</span>
                    <span class="value"><asp:Label ID="lblStatus" runat="server"></asp:Label></span>
                </div>
            </div>

            <div class="section">
                <h3>📝 Signature Locations (From Module 2 - Aryan)</h3>
                <div style="background: white; border: 1px solid #e0e0e0; border-radius: 8px; padding: 15px;">
                    <asp:Literal ID="litSignatures" runat="server"></asp:Literal>
                </div>
            </div>

            <div style="text-align: center; margin-top: 30px;">
                <asp:Button ID="btnApply" runat="server" 
                           Text="✓ Apply All Signatures to PDF" 
                           CssClass="btn btn-apply" 
                           OnClick="btnApply_Click" />
            </div>

            <div id="successPanel" runat="server" class="success-panel">
                <h3>✅ Signatures Applied Successfully!</h3>
                <p><asp:Label ID="lblSuccessMsg" runat="server"></asp:Label></p>
                <div style="margin-top: 15px;">
                    <asp:Button ID="btnDownload" runat="server" 
                               Text="📥 Download Signed PDF" 
                               CssClass="btn btn-download" 
                               OnClick="btnDownload_Click" />
                    <asp:Button ID="btnView" runat="server" 
                               Text="👁️ View Signed PDF" 
                               CssClass="btn btn-download" 
                               OnClick="btnView_Click" />
                </div>
            </div>
        </div>
    </form>
</body>
</html>
