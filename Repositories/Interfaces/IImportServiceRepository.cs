namespace WMS_Application.Repositories.Interfaces
{
    public interface IImportServiceRepository
    {
        byte[] GenerateSampleStockExcel(bool company);
        Task<byte[]> ShopProcessStockImport(IFormFile file, int userId, int shopId);
        Task<byte[]> CompanyProcessStockImport(IFormFile file, int companyId);
        Task<byte[]> EmployeeProcessImport(IFormFile file, int userId);
        byte[] GenerateEmployeeSampleExcel();
    }
}
