using Microsoft.Extensions.Options;
using WMS_Application.Models;
using WMS_Application.Repositories.Interfaces;
using MailKit.Net.Smtp;
using MimeKit;
using MailKit.Security;
namespace WMS_Application.Repositories.Auth
{
    public class EmailSenderRepository : IEmailSenderRepository
    {
        private readonly SmtpSettings _smtpSettings;
        
        public EmailSenderRepository(IOptions<SmtpSettings> smtpSettings)
        {
            _smtpSettings = smtpSettings.Value;
        }

        //This method will send email to user using Mailkit library
        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                // Creating a MailboxAddress with both the display name and the email address
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("OTP Verifier", "tester.vikash022@gmail.com"));
                message.To.Add(new MailboxAddress("User", toEmail));
                message.Subject = subject;
                message.Body = new TextPart("html") { Text = body };


                using (var smtpClient = new SmtpClient())
                {
                    await smtpClient.ConnectAsync(_smtpSettings.Server, _smtpSettings.Port, SecureSocketOptions.StartTls);
                    await smtpClient.AuthenticateAsync(_smtpSettings.SenderEmail, _smtpSettings.SenderPassword);
                    await smtpClient.SendAsync(message);
                    await smtpClient.DisconnectAsync(true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        //Email generation with File
        public async Task SendEmailAsync(string toEmail, string subject, string body, byte[] attachmentBytes = null, string attachmentFilename = null)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("TradeFlow", "tester.vikash022@gmail.com"));
                message.To.Add(new MailboxAddress("User", toEmail));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder { HtmlBody = body };

                // 🔥 Attach PDF if available
                if (attachmentBytes != null && !string.IsNullOrEmpty(attachmentFilename))
                {
                    bodyBuilder.Attachments.Add(attachmentFilename, attachmentBytes, ContentType.Parse("application/pdf"));
                }

                message.Body = bodyBuilder.ToMessageBody();

                using (var smtpClient = new SmtpClient())
                {
                    await smtpClient.ConnectAsync(_smtpSettings.Server, _smtpSettings.Port, SecureSocketOptions.StartTls);
                    await smtpClient.AuthenticateAsync(_smtpSettings.SenderEmail, _smtpSettings.SenderPassword);
                    await smtpClient.SendAsync(message);
                    await smtpClient.DisconnectAsync(true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
                throw;
            }
        }


        //We are generating OTP here which will be sent to user's email and saved in db
        public string GenerateOtp()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString();
        }

    }
}
