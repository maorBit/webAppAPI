using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
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
                bool isHebrew = IsHebrew(request.Message);כ

                string subject = isHebrew
                    ? $"ברכה מאת {request.PlayerName}"
                    : $"Greeting from {request.PlayerName}";

                string contentId = "SelfieImage";
                string tempFilePath = Path.Combine(Path.GetTempPath(), $"selfie_{Guid.NewGuid()}.png");
                System.IO.File.WriteAllBytes(tempFilePath, Convert.FromBase64String(request.ImageBase64));

                LinkedResource inlineImage = new LinkedResource(tempFilePath, "image/png")
                {
                    ContentId = contentId,
                    TransferEncoding = TransferEncoding.Base64,
                    ContentType = new ContentType("image/png"),
                    ContentLink = new Uri($"cid:{contentId}")
                };

                string htmlBody = $@"
                <body style='font-family:Arial,sans-serif; direction:{(isHebrew ? "rtl" : "ltr")}; text-align:{(isHebrew ? "right" : "left")}'>
                    <h2 style='color:#4A90E2;'>🎉 {(isHebrew ? "קיבלתם ברכה ממשחק האירוע!" : "You received a greeting from the invitation game!")}</h2>
                    <p><strong>{(isHebrew ? "מאת" : "From")}:</strong> {WebUtility.HtmlEncode(request.PlayerName)}</p>
                    <p><strong>{(isHebrew ? "הודעה" : "Message")}:</strong> {WebUtility.HtmlEncode(request.Message)}</p>
                    <p><strong>{(isHebrew ? "תוצאה" : "Score")}:</strong> {request.Score}</p>
                    <img src='cid:{contentId}' alt='Selfie' style='margin-top:20px;max-width:100%;height:auto;border-radius:8px;' />
                </body>";

                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(htmlBody, Encoding.UTF8, "text/html");
                htmlView.LinkedResources.Add(inlineImage);

                MailMessage mail = new MailMessage
                {
                    From = new MailAddress("jubilo.gamestudio@gmail.com", "Jubilo - Game Studio", Encoding.UTF8),
                    Subject = subject,
                    SubjectEncoding = Encoding.UTF8,
                    BodyEncoding = Encoding.UTF8,
                    IsBodyHtml = true
                };

                mail.To.Add(request.RecipientEmail);
                mail.AlternateViews.Add(htmlView);

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
