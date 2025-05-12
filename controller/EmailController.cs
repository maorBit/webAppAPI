using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
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
                bool isHebrew = IsHebrew(request.Message);

                string subject = isHebrew
                    ? $"ברכה מאת {request.PlayerName}"
                    : $"Greeting from {request.PlayerName}";

                string htmlBody = $@"
                    <div style='font-family: Arial, sans-serif; font-size: 16px; color: #333; direction: {(isHebrew ? "rtl" : "ltr")}; text-align: {(isHebrew ? "right" : "left")};'>
                        <h2 style='color: #0077cc;'>{(isHebrew ? "🎉 קיבלתם ברכה ממשחק האירוע!" : "🎉 You received a greeting from the invitation game!")}</h2>
                        <p><strong>👤 {(isHebrew ? "שם" : "Name")}:</strong> {request.PlayerName}</p>
                        <p><strong>🕒 {(isHebrew ? "תוצאה" : "Score")}:</strong> {request.Score}</p>
                        <p><strong>📨 {(isHebrew ? "הודעה" : "Message")}:</strong><br>{request.Message}</p>
                        <p><strong>📸 {(isHebrew ? "תמונה" : "Image")}:</strong></p>
                        <img src=""data:image/png;base64,{request.ImageBase64}"" style='max-width:100%; border-radius:8px;' />
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
