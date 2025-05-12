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
            if (request == null
                || string.IsNullOrWhiteSpace(request.Message)
                || string.IsNullOrWhiteSpace(request.ImageBase64)
                || string.IsNullOrWhiteSpace(request.RecipientEmail))
            {
                return BadRequest("Invalid request data.");
            }

            try
            {
                // נשתמש ישירות בבסיס64 להתרשמות בתמונה
                bool messageIsHebrew = IsHebrew(request.Message);
                bool nameIsHebrew = IsHebrew(request.PlayerName);

                string subject = nameIsHebrew
                    ? $"ברכה מאת {request.PlayerName}"
                    : $"Greeting from {request.PlayerName}";

                // בונים HTML מלא עם Charset ו־dir
                string htmlBody = $@"
<html>
  <head>
    <meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"" />
  </head>
  <body style=""margin:0;padding:0;font-family:Arial,sans-serif;font-size:16px;color:#333;direction:{(messageIsHebrew ? "rtl" : "ltr")};text-align:{(messageIsHebrew ? "right" : "left")};"">
    <h2 style=""color:#0077cc;margin-bottom:8px;"">
      {(messageIsHebrew
          ? "קיבלת ברכה ממשחק ההזמנה"
          : "You received a greeting from the invitation game")}
    </h2>
    <p><strong>👤 {(messageIsHebrew ? "שם" : "Name")}:</strong> {request.PlayerName}</p>
    <p><strong>🕒 {(messageIsHebrew ? "תוצאה" : "Score")}:</strong> {request.Score}</p>
    <p><strong>📨 {(messageIsHebrew ? "הודעה" : "Message")}:</strong><br/>{request.Message}</p>
    <p><strong>📸 {(messageIsHebrew ? "תמונה" : "Image")}:</strong></p>
    <img src=""data:image/png;base64,{request.ImageBase64}"" style=""max-width:100%;border-radius:8px;"" />
  </body>
</html>";

                var mail = new MailMessage
                {
                    From = new MailAddress(
                        "jubilo.gamestudio@gmail.com",
                        "Jubilo - Game Studio",
                        Encoding.UTF8),
                    Subject = subject,
                    SubjectEncoding = Encoding.UTF8,
                    BodyEncoding = Encoding.UTF8,
                    IsBodyHtml = true,
                    Body = htmlBody
                };

                mail.To.Add(request.RecipientEmail);

                using (var smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new NetworkCredential(
                        "jubilo.gamestudio@gmail.com",
                        "luyq azow wets wcdk");
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
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
            return !string.IsNullOrEmpty(text)
                && text.Any(c => c >= 0x0590 && c <= 0x05FF);
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
