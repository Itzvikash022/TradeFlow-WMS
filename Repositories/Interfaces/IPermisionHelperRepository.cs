using WMS_Application.Models;

namespace WMS_Application.Repositories.Interfaces
{
    public interface IPermisionHelperRepository
    {
        public string HasAccess(string tabName, int roleId);
    }
}
