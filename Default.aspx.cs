using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

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

            string filePath = Server.MapPath("~/Uploads/") + FileUpload1.FileName;
            FileUpload1.SaveAs(filePath);

            lblMessage.Text = "File uploaded successfully!";
            lblMessage.ForeColor = System.Drawing.Color.Green;

            ScriptManager.RegisterStartupScript(this, this.GetType(), "hideSpinner", "document.getElementById('loadingSpinner').style.display='none';", true);

            // Show preview
            pdfPreview.Attributes["src"] = "~/Uploads/" + FileUpload1.FileName;
        }
    }
}