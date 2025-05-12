using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Net;
using System.Net.Mail;

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
                byte[] imageBytes = Convert.FromBase64String(request.ImageBase64);
                string tempFilePath = Path.Combine(Path.GetTempPath(), $"selfie_{Guid.NewGuid()}.png");
                System.IO.File.WriteAllBytes(tempFilePath, imageBytes);

                MailMessage mail = new MailMessage
                {
                    From = new MailAddress("jubilo.gamestudio@gmail.com", "Jubilo - Game Studio"),
                    Subject = $"???? ??? {request.PlayerName}",
                    Body = $"{request.Message}\n\n?????: {request.Score}",
                    IsBodyHtml = false
                };
                mail.To.Add(request.RecipientEmail);
                mail.Attachments.Add(new Attachment(tempFilePath));

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
