using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Web.UI;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Kernel.Colors;
using Newtonsoft.Json;

namespace DigitalSignatureProject
{
    public partial class ApplySignature : Page
    {
        // Database connection string from Web.config
        private string connString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        private int documentId;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Get document ID from query string or session
                if (Request.QueryString["id"] != null)
                {
                    documentId = Convert.ToInt32(Request.QueryString["id"]);
                }
                else if (Session["DocumentId"] != null)
                {
                    documentId = Convert.ToInt32(Session["DocumentId"]);
                }
                else
                {
                    // For testing: use latest document
                    documentId = GetLatestDocumentId();
                }

                LoadDocumentInfo();
                LoadSignatures();
            }
        }

        private int GetLatestDocumentId()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    string query = "SELECT TOP 1 DocumentId FROM Documents ORDER BY DocumentId DESC";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    object result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : 1;
                }
            }
            catch
            {
                return 1; // Default for testing
            }
        }

        private void LoadDocumentInfo()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    string query = "SELECT * FROM Documents WHERE DocumentId = @Id";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Id", documentId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            lblDocId.Text = reader["DocumentId"].ToString();
                            lblFileName.Text = reader["FileName"].ToString();
                            lblPages.Text = reader["TotalPages"].ToString();
                            lblStatus.Text = reader["Status"].ToString();
                        }
                        else
                        {
                            lblDocId.Text = "N/A";
                            lblFileName.Text = "No document found";
                            lblStatus.Text = "Error";
                            btnApply.Enabled = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Error: " + ex.Message;
                btnApply.Enabled = false;
            }
        }

        private void LoadSignatures()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    string query = @"SELECT * FROM Signatures 
                                    WHERE DocumentId = @Id 
                                    ORDER BY SignatureId";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Id", documentId);

                    string html = "";
                    int count = 0;

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            count++;
                            string signer = reader["SignerName"].ToString();
                            double x = Convert.ToDouble(reader["CoordinateX"]);
                            double y = Convert.ToDouble(reader["CoordinateY"]);
                            int page = Convert.ToInt32(reader["PageNumber"]);

                            html += $@"<div class='signature-item'>
                                <strong>{count}. {signer}</strong>
                                <small style='display:block; margin-top:5px; color:#666;'>
                                    Position: X={x:F1}, Y={y:F1} | Page: {page}
                                </small>
                            </div>";
                        }
                    }

                    if (count == 0)
                    {
                        html = "<p style='color: #999; text-align: center; padding: 20px;'>⚠️ No signatures found. Please complete Module 2 (Aryan) first or add test data.</p>";
                        btnApply.Enabled = false;
                    }
                    else
                    {
                        html = $"<p style='margin-bottom:15px; color:#666;'><strong>Total Signatures:</strong> {count}</p>" + html;
                    }

                    litSignatures.Text = html;
                }
            }
            catch (Exception ex)
            {
                litSignatures.Text = $"<p style='color: red;'>Error loading signatures: {ex.Message}</p>";
                btnApply.Enabled = false;
            }
        }

        protected void btnApply_Click(object sender, EventArgs e)
        {
            try
            {
                // Get document ID from label
                int docId = Convert.ToInt32(lblDocId.Text);

                // 1. Apply signatures to PDF
                string signedFilePath = ApplySignaturesToPDF(docId);

                // 2. Update database
                UpdateDatabase(docId);

                // 3. Show success
                successPanel.Style["display"] = "block";
                lblSuccessMsg.Text = $"Successfully applied signatures to document. Signed PDF saved: {Path.GetFileName(signedFilePath)}";
                lblStatus.Text = "Completed ✅";
                btnApply.Enabled = false;

                // Store file path in session for download
                Session["SignedFilePath"] = signedFilePath;
            }
            catch (Exception ex)
            {
                Response.Write($"<script>alert('Error: {ex.Message}');</script>");
            }
        }

        private string ApplySignaturesToPDF(int docId)
        {
            // Get original PDF file
            string fileName = lblFileName.Text;

            // Ensure uploads folder exists
            string uploadsPath = Server.MapPath("~/Uploads");
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            string originalPath = Path.Combine(uploadsPath, fileName);
            string signedFileName = "signed_" + Path.GetFileNameWithoutExtension(fileName) + ".pdf";
            string signedPath = Path.Combine(uploadsPath, signedFileName);

            // For testing: create a simple PDF if original doesn't exist
            if (!File.Exists(originalPath))
            {
                CreateSamplePDF(originalPath);
            }

            // Get signatures from database
            List<SignatureData> signatures = GetSignaturesFromDB(docId);

            // Apply signatures using iText7
            using (PdfDocument pdfDoc = new PdfDocument(
                new PdfReader(originalPath),
                new PdfWriter(signedPath)))
            {
                Document document = new Document(pdfDoc);

                foreach (var sig in signatures)
                {
                    // Create signature text
                    Paragraph sigParagraph = new Paragraph($"[Signed: {sig.SignerName}]")
                        .SetFontSize(10)
                        .SetFontColor(ColorConstants.BLUE)
                        .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                        .SetPadding(5);

                    // Position signature on PDF
                    // Note: PDF coordinates start from bottom-left
                    float pageHeight = 842; // A4 height in points
                    sigParagraph.SetFixedPosition(
                        sig.PageNumber,
                        (float)sig.X,
                        pageHeight - (float)sig.Y,
                        150
                    );

                    document.Add(sigParagraph);

                    // Mark as signed in database
                    MarkAsSigned(sig.SignatureId);
                }

                document.Close();
            }

            return signedPath;
        }

        private void CreateSamplePDF(string path)
        {
            // Create sample PDF for testing
            using (PdfWriter writer = new PdfWriter(path))
            {
                using (PdfDocument pdf = new PdfDocument(writer))
                {
                    Document document = new Document(pdf);

                    document.Add(new Paragraph("SAMPLE DOCUMENT FOR DIGITAL SIGNATURE TESTING")
                        .SetFontSize(18)
                        
                        .SetMarginBottom(20));

                    document.Add(new Paragraph("Module 3 - Digital Signature Application")
                        .SetFontSize(14)
                        .SetMarginBottom(20));

                    document.Add(new Paragraph("This is a test document created for Module 3 testing."));
                    document.Add(new Paragraph("Developer: Janmesh"));
                    document.Add(new Paragraph("Date: " + DateTime.Now.ToString("dd-MMM-yyyy")));

                    document.Add(new Paragraph("\n\n\nSignature areas will be placed below:\n\n")
                        .SetMarginTop(40));

                    document.Close();
                }
            }
        }

        private List<SignatureData> GetSignaturesFromDB(int docId)
        {
            List<SignatureData> signatures = new List<SignatureData>();

            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                string query = "SELECT * FROM Signatures WHERE DocumentId = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", docId);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        signatures.Add(new SignatureData
                        {
                            SignatureId = Convert.ToInt32(reader["SignatureId"]),
                            SignerName = reader["SignerName"].ToString(),
                            X = Convert.ToDouble(reader["CoordinateX"]),
                            Y = Convert.ToDouble(reader["CoordinateY"]),
                            PageNumber = Convert.ToInt32(reader["PageNumber"])
                        });
                    }
                }
            }

            return signatures;
        }

        private void MarkAsSigned(int signatureId)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                string query = @"UPDATE Signatures 
                                SET IsSigned = 1, SignedDate = GETDATE() 
                                WHERE SignatureId = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", signatureId);
                cmd.ExecuteNonQuery();
            }
        }

        private void UpdateDatabase(int docId)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                string query = "UPDATE Documents SET Status = 'Completed' WHERE DocumentId = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", docId);
                cmd.ExecuteNonQuery();
            }
        }

        protected void btnDownload_Click(object sender, EventArgs e)
        {
            string filePath = Session["SignedFilePath"]?.ToString();

            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                Response.ContentType = "application/pdf";
                Response.AppendHeader("Content-Disposition",
                    "attachment; filename=signed_document.pdf");
                Response.TransmitFile(filePath);
                Response.End();
            }
            else
            {
                Response.Write("<script>alert('File not found!');</script>");
            }
        }

        protected void btnView_Click(object sender, EventArgs e)
        {
            string filePath = Session["SignedFilePath"]?.ToString();

            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                string fileName = Path.GetFileName(filePath);
                Response.Redirect($"~/Uploads/{fileName}");
            }
            else
            {
                Response.Write("<script>alert('File not found!');</script>");
            }
        }

        // Helper class for signature data
        private class SignatureData
        {
            public int SignatureId { get; set; }
            public string SignerName { get; set; }
            public double X { get; set; }
            public double Y { get; set; }
            public int PageNumber { get; set; }
        }
    }
}