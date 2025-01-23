namespace WMS_Application.Repositories.Interfaces
{
    public interface ILoginRepository
    {
        Task<object> AuthenticateUser(string emailOrUsername, string password);
        Task<object> TokenSenderViaEmail(string email);
        Task<object> ResetPassword(string creds, string newPassword);
    }
}
