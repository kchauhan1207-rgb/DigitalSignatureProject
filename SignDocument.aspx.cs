using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Globalization;

// ✅ Alias to avoid conflict with iTextSharp.text.ListItem
using WebListItem = System.Web.UI.WebControls.ListItem;

namespace DigitalSignatureProject
{
    public partial class SignDocument : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["PDFPath"] == null)
            {
                litMessage.Text = "<div style='color:red'>No PDF uploaded. Please upload first.</div>";
                return;
            }

            if (!IsPostBack)
            {
                int pageCount = Session["PDFPageCount"] != null ? (int)Session["PDFPageCount"] : 1;

                PopulatePageList(pageCount);

                if (cblPages.Items.Count > 0 &&
                    !cblPages.Items.Cast<WebListItem>().Any(i => i.Selected && i.Enabled))
                {
                    var firstEnabled = cblPages.Items.Cast<WebListItem>()
                        .FirstOrDefault(i => i.Enabled);

                    if (firstEnabled != null)
                        firstEnabled.Selected = true;
                }

                PopulateLocations();

                var requested = GetRequestedPagesFromSession();
                if (requested.Any())
                {
                    try
                    {
                        Session["PreviewPDF"] = CreatePreviewPdf(requested);
                    }
                    catch
                    {
                        Session["PreviewPDF"] = Session["PDFPath"];
                    }
                }
            }

            EmitSignedMarkersScript();
        }

        protected void cblPages_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ScriptManager.GetCurrent(Page) != null)
            {
                ScriptManager.RegisterStartupScript(
                    this,
                    GetType(),
                    "showSelectedAfterPostback",
                    "if(typeof showSelectedPage === 'function'){ showSelectedPage(); }",
                    true
                );
            }
        }

        private string CreatePreviewPdf(IEnumerable<int> pages)
        {
            string originalPath = (string)Session["PDFPath"];
            string previewPath = Path.Combine(
                Path.GetDirectoryName(originalPath),
                Path.GetFileNameWithoutExtension(originalPath) + "_preview.pdf"
            );

            using (PdfReader reader = new PdfReader(originalPath))
            using (FileStream fs = new FileStream(previewPath, FileMode.Create))
            using (Document doc = new Document())
            using (PdfCopy copy = new PdfCopy(doc, fs))
            {
                doc.Open();
                foreach (int p in pages.Distinct().OrderBy(x => x))
                {
                    if (p <= reader.NumberOfPages)
                        copy.AddPage(copy.GetImportedPage(reader, p));
                }
            }

            return previewPath;
        }

        private List<int> GetRequestedPagesFromSession()
        {
            return Session["SelectedPages"] as List<int> ?? new List<int>();
        }

        private void PopulatePageList(int pageCount)
        {
            cblPages.Items.Clear();
            var signed = Session["SignedPages"] as List<int> ?? new List<int>();

            var requested = GetRequestedPagesFromSession();
            IEnumerable<int> pagesToShow =
                requested.Any()
                    ? requested.Where(p => p >= 1 && p <= pageCount).Distinct().OrderBy(p => p)
                    : Enumerable.Range(1, pageCount);

            foreach (var p in pagesToShow)
            {
                var li = new WebListItem(p.ToString(), p.ToString());

                if (signed.Contains(p))
                {
                    li.Selected = true;
                    li.Enabled = false;
                }

                cblPages.Items.Add(li);
            }
        }

        protected void ddlOrg_SelectedIndexChanged(object sender, EventArgs e)
        {
            PopulateLocations();
        }

        protected void ddlVendor_SelectedIndexChanged(object sender, EventArgs e)
        {
            PopulateLocations();
        }

        private void PopulateLocations()
        {
            ddlLocation.Items.Clear();
            ddlLocation.Items.Add(new WebListItem("-- Choose Location --", ""));
            ddlLocation.Items.Add(new WebListItem("Top-Right", "Top-Right"));
            ddlLocation.Items.Add(new WebListItem("Bottom-Left", "Bottom-Left"));

            var map = new Dictionary<string, object>
            {
                { "Top-Right", new { x = 85, y = 10 } },
                { "Bottom-Left", new { x = 10, y = 90 } }
            };

            litLocationScript.Text =
                $"<script>var locationMap = {new JavaScriptSerializer().Serialize(map)};</script>";
        }

        protected void btnConfirmSign_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(hfSelectedPage.Value, out int page))
                return;

            string xPercentStr = (hfClickX.Value ?? "").Trim();
            string yPercentStr = (hfClickY.Value ?? "").Trim();
            if (string.IsNullOrEmpty(xPercentStr) || string.IsNullOrEmpty(yPercentStr))
                return;

            var records = Session["SignatureRecords"] as List<Dictionary<string, string>>
                          ?? new List<Dictionary<string, string>>();

            records.Add(new Dictionary<string, string>
            {
                { "Page", page.ToString(CultureInfo.InvariantCulture) },
                { "XPercent", xPercentStr },
                { "YPercent", yPercentStr },
                { "Organization", ddlOrg.SelectedValue ?? "" },
                { "Vendor", ddlVendor.SelectedValue ?? "" },
                { "Location", ddlLocation.SelectedValue ?? "" }
            });

            Session["SignatureRecords"] = records;

            var signed = Session["SignedPages"] as List<int> ?? new List<int>();
            if (!signed.Contains(page))
                signed.Add(page);
            Session["SignedPages"] = signed;

            try
            {
                string originalPath = (string)Session["PDFPath"];
                string signedPath = StampSignatureOnPdf(
                    originalPath,
                    page,
                    xPercentStr,
                    yPercentStr,
                    $"{ddlOrg.SelectedValue} - {ddlVendor.SelectedValue}"
                );

                if (!string.IsNullOrEmpty(signedPath))
                {
                    Session["PDFPath"] = signedPath;
                    Session["PreviewPDF"] = null;
                }
            }
            catch (Exception ex)
            {
                litMessage.Text =
                    $"<div style='color:orange'>Signature saved, but PDF stamping failed: {Server.HtmlEncode(ex.Message)}</div>";
            }

            int pageCount = Session["PDFPageCount"] != null ? (int)Session["PDFPageCount"] : 1;
            PopulatePageList(pageCount);

            foreach (WebListItem li in cblPages.Items)
                li.Selected = false;

            var firstEnabled = cblPages.Items.Cast<WebListItem>().FirstOrDefault(i => i.Enabled);
            if (firstEnabled != null)
                firstEnabled.Selected = true;
            else
                litMessage.Text = "<div style='color:green'>All selected pages have been signed.</div>";

            EmitSignedMarkersScript();

            if (ScriptManager.GetCurrent(Page) != null)
            {
                ScriptManager.RegisterStartupScript(
                    this,
                    GetType(),
                    "updateClientAfterSign",
                    "if(typeof showSelectedPage === 'function'){ showSelectedPage(); }",
                    true
                );
            }
        }

        private void EmitSignedMarkersScript()
        {
            var records = Session["SignatureRecords"] as List<Dictionary<string, string>> ?? new List<Dictionary<string, string>>();
            litSignedScript.Text =
                $"<script>var signedMarkers = {new JavaScriptSerializer().Serialize(records)};</script>";
        }

        private string StampSignatureOnPdf(
            string originalPath,
            int page,
            string xPercentStr,
            string yPercentStr,
            string signatureText)
        {
            if (string.IsNullOrEmpty(originalPath) || !File.Exists(originalPath))
                return null;

            string dir = Path.GetDirectoryName(originalPath);
            string signedPath = Path.Combine(
                dir,
                Path.GetFileNameWithoutExtension(originalPath) + "_signed.pdf"
            );

            if (!double.TryParse(xPercentStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double xPct))
                return null;

            if (!double.TryParse(yPercentStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double yPct))
                return null;

            xPct = Math.Max(0, Math.Min(100, xPct));
            yPct = Math.Max(0, Math.Min(100, yPct));

            using (PdfReader reader = new PdfReader(originalPath))
            using (FileStream fs = new FileStream(signedPath, FileMode.Create, FileAccess.Write))
            using (PdfStamper stamper = new PdfStamper(reader, fs))
            {
                if (page < 1 || page > reader.NumberOfPages)
                    return null;

                var pageSize = reader.GetPageSize(page);

                float x = (float)(pageSize.Width * (xPct / 100.0));
                float y = (float)(pageSize.Height * (1.0 - yPct / 100.0));

                PdfContentByte over = stamper.GetOverContent(page);
                BaseFont bf = BaseFont.CreateFont(
                    BaseFont.HELVETICA,
                    BaseFont.CP1252,
                    BaseFont.NOT_EMBEDDED
                );

                float radius = 10f;

                // Red circle
                over.SaveState();
                over.SetRGBColorFill(220, 53, 69);
                over.Circle(x, y, radius);
                over.Fill();
                over.RestoreState();

                // White "S"
                over.BeginText();
                over.SetFontAndSize(bf, 8);
                over.SetColorFill(BaseColor.WHITE);
                over.ShowTextAligned(Element.ALIGN_CENTER, "S", x, y - 3, 0);
                over.EndText();

                // Black signature text
                over.BeginText();
                over.SetFontAndSize(bf, 9);
                over.SetColorFill(BaseColor.BLACK);
                over.ShowTextAligned(
                    Element.ALIGN_LEFT,
                    signatureText,
                    x + radius + 4,
                    y - 4,
                    0
                );
                over.EndText();
            }

            return signedPath;
        }
    }
}