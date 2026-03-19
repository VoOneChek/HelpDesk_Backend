using Application.Common.Result;
using Application.DTOs.Report;

namespace Application.Abstraction
{
    public interface IReportService
    {
        Task<Result<ReportResultDto>> GetReportAsync(ReportFilterDto filter);
        Task<Result<(byte[] FileBytes, string FileName, string ContentType)>> ExportReportAsync(ReportFilterDto filter, string format);
    }
}
