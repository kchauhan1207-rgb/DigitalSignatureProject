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

            LoadSelectedPages();
        }

        /// <summary>
        /// Loads selected PDF pages from session and binds them to UI
        /// </summary>
        private void LoadSelectedPages()
        {
            // Read session values safely
            int[] selectedPages = Session["SelectedPages"] as int[];
            string pdfPath = Session["PDFPath"] as string;

            // Validate selected pages
            if (selectedPages == null || selectedPages.Length == 0)
            {
                ShowError("No pages selected. Please go back and select pages to sign.");
                return;
            }

            // Validate PDF path
            if (string.IsNullOrWhiteSpace(pdfPath))
            {
                ShowError("Uploaded PDF not found. Please upload the document again.");
                return;
            }

            // Ensure file exists on server
            string fileName = Path.GetFileName(pdfPath);
            string serverPath = Server.MapPath("~/Uploads/" + fileName);

            if (!File.Exists(serverPath))
            {
                ShowError("PDF file does not exist on the server.");
                return;
            }

            // Resolve virtual path for browser
            string virtualPath = ResolveUrl("~/Uploads/" + fileName);

            // Build page links
            pagesList.InnerHtml = BuildPageLinks(selectedPages, virtualPath);

            // Load first selected page by default
            pdfFrame.Attributes["src"] = virtualPath + "#page=" + selectedPages[0];
        }

        /// <summary>
        /// Builds clickable page links for selected pages
        /// </summary>
        private string BuildPageLinks(int[] pages, string pdfUrl)
        {
            StringBuilder sb = new StringBuilder();

            foreach (int pageNumber in pages)
            {
                sb.AppendFormat(
                    "<a href=\"{0}#page={1}\" target=\"pdfFrame\">Page {1}</a>&nbsp;&nbsp;",
                    HttpUtility.HtmlEncode(pdfUrl),
                    pageNumber
                );
            }

            return sb.ToString();
        }

        /// <summary>
        /// Displays error message to user
        /// </summary>
        private void ShowError(string message)
        {
            litMessage.Text =
                $"<div style='color:darkred; font-weight:600'>{HttpUtility.HtmlEncode(message)}</div>";

            pagesList.InnerHtml = string.Empty;
            pdfFrame.Attributes["src"] = string.Empty;
        }
    }
}
