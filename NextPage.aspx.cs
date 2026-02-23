using System;
using System.IO;
using System.Text;
using System.Web.UI;

namespace DigitalSignatureProject
{
    public partial class NextPage : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                int[] selected = Session["SelectedPages"] as int[];
                string pdfPath = Session["PDFPath"] as string;

                if (selected == null || selected.Length == 0)
                {
                    litMessage.Text =
                        "<div style='color:darkred'>No pages selected. Go back and select pages to sign.</div>";
                    return;
                }

                if (string.IsNullOrEmpty(pdfPath))
                {
                    litMessage.Text =
                        "<div style='color:darkred'>Uploaded PDF not found in session.</div>";
                    return;
                }

                string fileName = Path.GetFileName(pdfPath);
                string virtualPath = ResolveUrl("~/Uploads/" + fileName);

                StringBuilder sb = new StringBuilder();

                foreach (int p in selected)
                {
                    sb.AppendFormat(
                        "<a href=\"{0}#page={1}\" target=\"pdfFrame\">Open page {1}</a>&nbsp;&nbsp;",
                        virtualPath,
                        p
                    );
                }

                pagesList.InnerHtml = sb.ToString();
                pdfFrame.Attributes["src"] = virtualPath + "#page=" + selected[0];
            }
        }
    }
}