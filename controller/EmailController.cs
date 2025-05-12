using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace EmailWebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailController : ControllerBase
    {
        [HttpPost("send-email")]
        public async Task<IActionResult> SendEmail([FromBody] EmailRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Message) ||
                string.IsNullOrEmpty(request.ImageBase64) || string.IsNullOrEmpty(request.RecipientEmail))
            {
                return BadRequest("Invalid request data.");
            }

            try
            {
                // Decode the base64 image
                byte[] imageBytes = Convert.FromBase64String(request.ImageBase64);

                // Save image temporarily
                string tempFilePath = Path.Combine(Path.GetTempPath(), "selfie.png");
                await System.IO.File.WriteAllBytesAsync(tempFilePath, imageBytes);

                // Read into memory to avoid locking the file
                Attachment imageAttachment;
                using (FileStream fs = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read))
                {
                    MemoryStream ms = new MemoryStream();
                    await fs.CopyToAsync(ms);
                    ms.Position = 0;
                    imageAttachment = new Attachment(ms, "selfie.png", "image/png");
                }

                // Delete the temp file after copying
                System.IO.File.Delete(tempFilePath);

                // Prepare the email
                MailMessage mail = new MailMessage
                {
                    From = new MailAddress("jubilo.gamestudio@gmail.com", "Jubilo - Game Studio"),
                    Subject = $"Greetings from {request.PlayerName}!",
                    Body = $"{request.Message}\n\nScore: {request.Score}",
                    IsBodyHtml = false
                };
                mail.To.Add(request.RecipientEmail);
                mail.Attachments.Add(imageAttachment);

                // Send the email
                using (SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587))
                {
                    string? emailUser = Environment.GetEnvironmentVariable("EMAIL_USER");
                    string? emailPass = Environment.GetEnvironmentVariable("EMAIL_PASS");

                    if (string.IsNullOrEmpty(emailUser) || string.IsNullOrEmpty(emailPass))
                    {
                        return StatusCode(500, "Email credentials are not configured properly.");
                    }

                    smtpClient.Credentials = new NetworkCredential(emailUser, emailPass);
                    smtpClient.EnableSsl = true;
                    smtpClient.Send(mail);
                }

                return Ok("Email sent successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }

    public class EmailRequest
    {
        public string Message { get; set; } = string.Empty;
        public string PlayerName { get; set; } = string.Empty;
        public string Score { get; set; } = string.Empty;
        public string ImageBase64 { get; set; } = string.Empty;
        public string RecipientEmail { get; set; } = string.Empty;
    }
}
