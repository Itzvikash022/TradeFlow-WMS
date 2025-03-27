using WMS_Application.Repositories.Interfaces;
using System.Data;
using System.IO;
using System.Linq;
using ClosedXML.Excel;

namespace WMS_Application.Repositories
{
    public class ExportServiceRepository : IExportServiceRepository
    {
        public byte[] ExportToExcel(DataTable dataTable, string sheetName)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(sheetName);
            worksheet.Cell(1, 1).InsertTable(dataTable);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}
