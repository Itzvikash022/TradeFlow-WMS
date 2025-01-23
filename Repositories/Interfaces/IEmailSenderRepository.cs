namespace WMS_Application.Repositories.Interfaces
{
    public interface IEmailSenderRepository
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
        string GenerateOtp();
    }
}
