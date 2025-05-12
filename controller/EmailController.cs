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
            if (request == null || string.IsNullOrWhiteSpace(request.Message) ||
                string.IsNullOrWhiteSpace(request.ImageBase64) || string.IsNullOrWhiteSpace(request.RecipientEmail))
            {
                return BadRequest("Invalid request data.");
            }

            try
            {
                byte[] imageBytes = Convert.FromBase64String(request.ImageBase64);
                string tempFilePath = Path.Combine(Path.GetTempPath(), $"selfie_{Guid.NewGuid()}.png");
                System.IO.File.WriteAllBytes(tempFilePath, imageBytes);

                string subject = IsHebrew(request.PlayerName) ?
                    $"ברכה מאת {request.PlayerName}" :
                    $"Greeting from {request.PlayerName}";

                string htmlBody = $@"
                    <div style='font-family: Arial, sans-serif; font-size: 16px; color: #333; direction: {(IsHebrew(request.Message) ? "rtl" : "ltr")}; text-align: {(IsHebrew(request.Message) ? "right" : "left")};'>
                        <h2 style='color: #0077cc;'>{(IsHebrew(request.Message) ? "קיבלת ברכה ממשחק ההזמנה" : "You received a greeting from the invitation game")}</h2>
                        <p><strong>👤 {(IsHebrew(request.Message) ? "שם" : "Name")}:</strong> {request.PlayerName}</p>
                        <p><strong>🕒 {(IsHebrew(request.Message) ? "תוצאה" : "Score")}:</strong> {request.Score}</p>
                        <p><strong>📨 {(IsHebrew(request.Message) ? "הודעה" : "Message")}:</strong><br>{request.Message}</p>
                        <p><strong>📸 {(IsHebrew(request.Message) ? "תמונה" : "Image")}:</strong></p>
                        <img src='data:image/png;base64,{request.ImageBase64}' style='max-width:100%; border-radius:8px;' />
                    </div>";

                MailMessage mail = new MailMessage
                {
                    From = new MailAddress("jubilo.gamestudio@gmail.com", "Jubilo - Game Studio", Encoding.UTF8),
                    Subject = subject,
                    SubjectEncoding = Encoding.UTF8,
                    BodyEncoding = Encoding.UTF8,
                    IsBodyHtml = true,
                    Body = htmlBody
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

        private static bool IsHebrew(string text)
        {
            return !string.IsNullOrEmpty(text) && text.Any(c => c >= 0x0590 && c <= 0x05FF);
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
