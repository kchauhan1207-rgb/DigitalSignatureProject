using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using iTextSharp.text.pdf; // Requires iTextSharp NuGet package

namespace DigitalSignatureProject
{
    public partial class _Default : Page
    {
        protected void btnUpload_Click(object sender, EventArgs e)
        {
            // show spinner before upload starts
            ScriptManager.RegisterStartupScript(this, this.GetType(), "hideSpinner", "document.getElementById('loadingSpinner').style.display='none';", true);

            if (!FileUpload1.HasFile)
            {
                lblMessage.Text = "Please select a file!";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                ScriptManager.RegisterStartupScript(this, this.GetType(), "hideSpinner", "document.getElementById('loadingSpinner').style.display='none';", true);
                return;
            }

            string fileExtension = System.IO.Path.GetExtension(FileUpload1.FileName).ToLower();
            if (fileExtension != ".pdf")
            {
                lblMessage.Text = "Only PDF files are allowed!";
                lblMessage.ForeColor = System.Drawing.Color.Red;

                ScriptManager.RegisterStartupScript(this, this.GetType(), "hideSpinner", "document.getElementById('loadingSpinner').style.display='none';", true);
                return;
            }

            string uploadsDir = Server.MapPath("~/Uploads/");
            if (!System.IO.Directory.Exists(uploadsDir))
            {
                System.IO.Directory.CreateDirectory(uploadsDir);
            }

            string filePath = System.IO.Path.Combine(uploadsDir, FileUpload1.FileName);
            FileUpload1.SaveAs(filePath);

            // Determine page count using iTextSharp
            int pageCount = 1;
            try
            {
                using (PdfReader reader = new PdfReader(filePath))
                {
                    pageCount = reader.NumberOfPages;
                }
            }
            catch
            {
                // If page count fails, default to 1
                pageCount = 1;
            }

            lblMessage.Text = "File uploaded successfully!";
            lblMessage.ForeColor = System.Drawing.Color.Green;

            Session["PDFPath"] = filePath;
            Session["PDFPageCount"] = pageCount;
            Session["SignedPages"] = new System.Collections.Generic.List<int>();

            Response.Redirect("SignDocument.aspx");

            ScriptManager.RegisterStartupScript(this, this.GetType(), "hideSpinner", "document.getElementById('loadingSpinner').style.display='none';", true);
        }
    }
}