namespace WMS_Application.Repositories.Interfaces
{
    public interface IActivityRepository
    {
        object AddNewActivity(int id, int roleId, string type, string desc);
    }
}
