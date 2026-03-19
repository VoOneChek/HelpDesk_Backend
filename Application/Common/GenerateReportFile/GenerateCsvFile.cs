using Application.DTOs.Report;
using CsvHelper;
using System.Globalization;

namespace Application.Common.GenerateReportFile
{
    public class GenerateCsvFile
    {
        public byte[] GenerateCsv(List<ReportItemDto> data)
        {
            using (var memoryStream = new MemoryStream())
            using (var writer = new StreamWriter(memoryStream))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.Context.RegisterClassMap<ReportItemMap>();

                csv.WriteRecords(data);
                writer.Flush();
                return memoryStream.ToArray();
            }
        }
    }
}
