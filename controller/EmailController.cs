﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EmailWebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailController : ControllerBase
    {
        private readonly ISendGridClient _sendGrid;
        private readonly IConfiguration _config;

        public EmailController(ISendGridClient sendGrid, IConfiguration config)
        {
            _sendGrid = sendGrid;
            _config = config;
        }

        [HttpPost("send-email")]
        public async Task<IActionResult> SendEmail([FromBody] EmailRequest request)
        {
            if (request == null
                || string.IsNullOrWhiteSpace(request.RecipientEmail)
                || string.IsNullOrWhiteSpace(request.Message)
                || string.IsNullOrWhiteSpace(request.ImageBase64))
            {
                return BadRequest("❌ נתוני הבקשה אינם תקינים (RecipientEmail, Message, ImageBase64)");
            }

            try
            {
                // Detect Hebrew
                bool isHebrew = request.Message.Any(c => c >= 0x0590 && c <= 0x05FF);

                string subject = isHebrew
                    ? $"ברכה מאת {request.PlayerName}"
                    : $"Greeting from {request.PlayerName}";

                string htmlContent = $@"
<html>
  <body style='font-family:Arial,sans-serif; direction:{(isHebrew ? "rtl" : "ltr")}; text-align:{(isHebrew ? "right" : "left")};'>
    <h2>🎉 {(isHebrew ? "קיבלתם ברכה ממשחק האירוע!" : "You received a greeting from the invitation game!")}</h2>
    <p><strong>{(isHebrew ? "מאת" : "From")}:</strong> {request.PlayerName}</p>
    <p><strong>{(isHebrew ? "הודעה" : "Message")}:</strong> {request.Message}</p>
    <p><strong>{(isHebrew ? "תוצאה" : "Score")}:</strong> {request.Score}</p>
    <img src='cid:selfie' alt='Selfie' style='margin-top:20px;max-width:100%;height:auto;border-radius:8px;' />
  </body>
</html>";

                var from = new EmailAddress(
                    _config["SendGrid:SenderEmail"],
                    _config["SendGrid:SenderName"]
                );
                var to = new EmailAddress(request.RecipientEmail);

                var msg = MailHelper.CreateSingleEmail(
                    from,
                    to,
                    subject,
                    plainTextContent: request.Message,
                    htmlContent: htmlContent
                );

                // Inline image
                msg.AddAttachment(
                    filename: "selfie.png",
                    base64Content: request.ImageBase64,
                    type: "image/png",
                    disposition: "inline",
                    content_id: "selfie"
                );

                var response = await _sendGrid.SendEmailAsync(msg);

                if (response.IsSuccessStatusCode)
                    return Ok(new { success = true, message = "Email sent via SendGrid!" });

                var errorBody = await response.Body.ReadAsStringAsync();
                Console.WriteLine($"❌ SendGrid error: {errorBody}");
                return StatusCode((int)response.StatusCode, new
                {
                    success = false,
                    error = "SendGrid failed to send the email.",
                    detail = errorBody
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔥 Internal error sending email: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    error = "Unexpected server error.",
                    detail = ex.Message
                });
            }
        }
    }

    public class EmailRequest
    {
        public string RecipientEmail { get; set; } = "";
        public string Message { get; set; } = "";
        public string PlayerName { get; set; } = "";
        public string Score { get; set; } = "";
        public string ImageBase64 { get; set; } = "";
    }
}
