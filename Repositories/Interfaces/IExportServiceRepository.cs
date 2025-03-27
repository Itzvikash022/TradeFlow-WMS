using System.Data;

namespace WMS_Application.Repositories.Interfaces
{
    public interface IExportServiceRepository
    {
        byte[] ExportToExcel(DataTable dataTable, string sheetName);
    }
}
