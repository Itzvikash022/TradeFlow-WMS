namespace WMS_Application.Repositories.Interfaces
{
    public interface LoginInterface
    {
        Task<object> AuthenticateUser(string emailOrUsername, string password);
    }
}
