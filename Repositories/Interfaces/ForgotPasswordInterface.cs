using WMS_Application.Models;

namespace WMS_Application.Repositories.Interfaces
{
    public interface ForgotPasswordInterface
    {
        Task<object> TokenSenderViaEmail(string email);
        Task<object> ResetPassword(string creds, string newPassword);
    }
}
