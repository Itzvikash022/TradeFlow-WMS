using Microsoft.Extensions.Options;
using WMS_Application.Models;
using WMS_Application.Repositories.Interfaces;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;
using MailKit.Security;
namespace WMS_Application.Repositories.Classes
{
    public class EmailSenderClass : EmailSenderInterface
    {
        private readonly SmtpSettings _smtpSettings;

        public EmailSenderClass(IOptions<SmtpSettings> smtpSettings)
        {
            _smtpSettings = smtpSettings.Value;
        }

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

        public string GenerateOtp()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString();
        }

    }
}
