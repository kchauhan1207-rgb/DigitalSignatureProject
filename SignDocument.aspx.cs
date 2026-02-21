using System;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;

namespace DigitalSignatureProject
{
    public partial class NextPage : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack)
                return;
    
            int[] selected = Session["SelectedPages"] as int[];
            object pdfPathObj = Session["PDFPath"];

            if (selected == null || selected.Length == 0)
            {
                litMessage.Text = "<div style='color:darkred'>No pages selected. Go back and select pages to sign.</div>";
                return;
            }

            if (pdfPathObj == null)
            {
                litMessage.Text = "<div style='color:darkred'>Uploaded PDF not found in session.</div>";
                return;
            }

            string physicalPath = pdfPathObj.ToString();

            if (!File.Exists(physicalPath))
            {
                litMessage.Text = "<div style='color:darkred'>PDF file does not exist on server.</div>";
                return;
            }

            string fileName = Path.GetFileName(physicalPath);
            string virtualPath = ResolveUrl("~/Uploads/" + HttpUtility.UrlEncode(fileName));

            StringBuilder sb = new StringBuilder();

            foreach (int p in selected)
            {
                sb.AppendFormat(
                    "<a href=\"#\" onclick=\"document.getElementById('{0}').src='{1}#page={2}'; return false;\">Open page {2}</a>&nbsp;&nbsp;",
                    pdfFrame.ClientID,
                    virtualPath,
                    p
                );
            }

            pagesList.InnerHtml = sb.ToString();
            pdfFrame.Attributes["src"] = virtualPath + "#page=" + selected[0];
        }
    }
}