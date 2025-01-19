namespace WMS_Application.Repositories.Interfaces
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
        string GenerateOtp();
    }
}
