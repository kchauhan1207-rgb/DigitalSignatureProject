    using System;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using iTextSharp.text.pdf;

    namespace DigitalSignatureProject
    {
        public partial class SignDocument : Page
        {
            protected void Page_Load(object sender, EventArgs e)
            {
                if (!IsPostBack)
                {
                    PopulatePagesFromSessionPdf();
                }
            }

            private void PopulatePagesFromSessionPdf()
            {
                try
                {
                    if (Session["PDFPath"] == null)
                        return;

                    string pdfPath = Session["PDFPath"].ToString();
                    if (string.IsNullOrEmpty(pdfPath))
                        return;

                    using (var reader = new PdfReader(pdfPath))
                    {
                        int pageCount = reader.NumberOfPages;
                        cblPages.Items.Clear();
                        for (int i = 1; i <= pageCount; i++)
                        {
                            cblPages.Items.Add(new ListItem($"Page {i}", i.ToString()));
                        }
                    }
                }
                catch (Exception ex)
                {
                    // If desired, surface the error in UI; for now we silently fail.
                    // Example: ScriptManager.RegisterStartupScript(this, this.GetType(), "err", $"alert('{ex.Message.Replace("'", "\\'")}');", true);
                }
            }

            protected void btnNext_Click(object sender, EventArgs e)
            {
                // Capture selected pages if needed
                var selectedPages = new System.Collections.Generic.List<int>();
                foreach (ListItem item in cblPages.Items)
                {
                    if (item.Selected)
                    {
                        if (int.TryParse(item.Value, out int pageNo))
                            selectedPages.Add(pageNo);
                    }
                }

                // Example: store selected pages in session for next page to consume
                Session["SelectedPages"] = selectedPages.ToArray();

                // Example: redirect
                Response.Redirect("NextPage.aspx");
            }
        }
    }
