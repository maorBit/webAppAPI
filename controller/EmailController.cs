using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace EmailWebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailController : ControllerBase
    {
        [HttpPost("send-email")]
        public IActionResult SendEmail([FromBody] EmailRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Message) ||
                string.IsNullOrEmpty(request.ImageBase64) || string.IsNullOrEmpty(request.RecipientEmail))
            {
                return BadRequest("Invalid request data.");
            }

            try
            {
                // Save base64 image to a temporary file (optional - for debugging only)
                byte[] imageBytes = Convert.FromBase64String(request.ImageBase64);
                string tempFilePath = Path.Combine(Path.GetTempPath(), $"selfie_{Guid.NewGuid()}.png");
                System.IO.File.WriteAllBytes(tempFilePath, imageBytes);

                // Compose the email
                MailMessage mail = new MailMessage
                {
                    From = new MailAddress("jubilo.gamestudio@gmail.com", "Jubilo - Game Studio", Encoding.UTF8),
                    Subject = $"???? ??? {request.PlayerName}",
                    SubjectEncoding = Encoding.UTF8,
                    BodyEncoding = Encoding.UTF8,
                    IsBodyHtml = true,
                    Body = $@"
                    <div style='font-family: Arial, sans-serif; font-size: 16px; color: #333; direction: rtl;'>
                        <h2 style='color: #0077cc;'>????? ???? ????? ??????</h2>
                        <p><strong>?? ??:</strong> {request.PlayerName}</p>
                        <p><strong>?? ?????:</strong> {request.Score}</p>
                        <p><strong>?? ?????:</strong><br>{request.Message}</p>
                        <p><strong>?? ?????:</strong></p>
                        <img src='data:image/png;base64,{request.ImageBase64}' style='max-width:100%; border-radius:8px;' />
                    </div>"
                };

                mail.To.Add(request.RecipientEmail);

                using (SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtpClient.Credentials = new NetworkCredential("jubilo.gamestudio@gmail.com", "luyq azow wets wcdk");
                    smtpClient.EnableSsl = true;
                    smtpClient.Send(mail);
                }

                System.IO.File.Delete(tempFilePath);
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
        public string Message { get; set; } = "";
        public string PlayerName { get; set; } = "";
        public string Score { get; set; } = "";
        public string ImageBase64 { get; set; } = "";
        public string RecipientEmail { get; set; } = "";
    }
}
