namespace WMS_Application.Repositories.Interfaces
{
    public interface EmailSenderInterface
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
        string GenerateOtp();
    }
}
