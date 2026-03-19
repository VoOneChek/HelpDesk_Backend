using Application.DTOs.Report;
using CsvHelper.Configuration;

namespace Application.Common.GenerateReportFile
{
    public sealed class ReportItemMap : ClassMap<ReportItemDto>
    {
        public ReportItemMap()
        {
            Map(m => m.TicketId).Name("ID");
            Map(m => m.Title).Name("Тема");
            Map(m => m.Status).Name("Статус");
            Map(m => m.CreatedAt).Name("Дата создания");
            Map(m => m.ClosedAt).Name("Дата закрытия");
            Map(m => m.CategoryName).Name("Категория");
            Map(m => m.ClientName).Name("Клиент");
            Map(m => m.OperatorName).Name("Оператор");
        }
    }
}
