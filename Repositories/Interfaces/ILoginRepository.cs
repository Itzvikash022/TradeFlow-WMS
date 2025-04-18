namespace WMS_Application.Repositories.Interfaces
{
    public interface ILoginRepository
    {
        Task<object> AuthenticateUser(string emailOrUsername, string password);
        Task<object> TokenSenderViaEmail(string email, string isCompany);
        Task<object> ResetPassword(string creds, string newPassword, bool isCompany);
    }
}
