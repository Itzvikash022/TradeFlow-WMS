namespace WMS_Application.Repositories.Interfaces
{
    public interface IEmailSenderRepository
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
        Task SendEmailAsync(string toEmail, string subject, string body, byte[] attachmentBytes, string attachmentFilename);
        string GenerateOtp();
    }
}
