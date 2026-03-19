using Application.DTOs.Report;
using ClosedXML.Excel;

namespace Application.Common.GenerateReportFile
{
    public class GenerateExcelFile
    {
        public byte[] GenerateExcelWithStats(ReportSummaryDto summary, List<ReportItemDto> details)
        {
            using (var workbook = new XLWorkbook())
            {
                // --- Лист 1: Сводка ---
                var summarySheet = workbook.Worksheets.Add("Сводка");

                summarySheet.Cell(1, 1).Value = "Отчет по работе техподдержки";
                summarySheet.Cell(1, 1).Style.Font.Bold = true;
                summarySheet.Cell(1, 1).Style.Font.FontSize = 16;

                summarySheet.Cell(3, 1).Value = "Всего обращений:";
                summarySheet.Cell(3, 2).Value = summary.TotalTickets;

                summarySheet.Cell(4, 1).Value = "Закрыто:";
                summarySheet.Cell(4, 2).Value = summary.ClosedTickets;

                summarySheet.Cell(5, 1).Value = "В работе/Открыто:";
                summarySheet.Cell(5, 2).Value = summary.OpenTickets;

                summarySheet.Cell(6, 1).Value = "Среднее время решения:";
                summarySheet.Cell(6, 2).Value = summary.AverageResolutionTime;

                // Топ категорий
                summarySheet.Cell(8, 1).Value = "Топ категорий";
                summarySheet.Cell(8, 1).Style.Font.Bold = true;
                int row = 9;
                foreach (var cat in summary.TopCategories)
                {
                    summarySheet.Cell(row, 1).Value = cat.CategoryName;
                    summarySheet.Cell(row, 2).Value = cat.Count;
                    row++;
                }

                // Эффективность операторов
                summarySheet.Cell(row + 1, 1).Value = "Эффективность операторов";
                summarySheet.Cell(row + 1, 1).Style.Font.Bold = true;
                row += 2;
                foreach (var op in summary.OperatorPerformance)
                {
                    summarySheet.Cell(row, 1).Value = op.OperatorName;
                    summarySheet.Cell(row, 2).Value = op.ClosedCount;
                    row++;
                }

                summarySheet.Columns().AdjustToContents();

                // --- Лист 2: Детализация ---
                var detailsSheet = workbook.Worksheets.Add("Детализация");

                // Заголовки
                detailsSheet.Cell(1, 1).Value = "ID";
                detailsSheet.Cell(1, 2).Value = "Тема";
                detailsSheet.Cell(1, 3).Value = "Статус";
                detailsSheet.Cell(1, 4).Value = "Приоритет";
                detailsSheet.Cell(1, 5).Value = "Создано";
                detailsSheet.Cell(1, 6).Value = "Закрыто";
                detailsSheet.Cell(1, 7).Value = "Время решения";
                detailsSheet.Cell(1, 8).Value = "Категория";
                detailsSheet.Cell(1, 9).Value = "Оператор";

                var headerRange = detailsSheet.Range(1, 1, 1, 9);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                // Данные
                for (int i = 0; i < details.Count; i++)
                {
                    var r = i + 2;
                    var item = details[i];

                    detailsSheet.Cell(r, 1).Value = item.TicketId.ToString();
                    detailsSheet.Cell(r, 2).Value = item.Title;
                    detailsSheet.Cell(r, 3).Value = item.Status;
                    detailsSheet.Cell(r, 4).Value = item.Priority;
                    detailsSheet.Cell(r, 5).Value = item.CreatedAt.ToString("g");
                    detailsSheet.Cell(r, 6).Value = item.ClosedAt?.ToString("g") ?? "-";
                    detailsSheet.Cell(r, 7).Value = item.ResolutionTime;
                    detailsSheet.Cell(r, 8).Value = item.CategoryName;
                    detailsSheet.Cell(r, 9).Value = item.OperatorName ?? "-";
                }

                detailsSheet.Columns().AdjustToContents();

                using (var memoryStream = new MemoryStream())
                {
                    workbook.SaveAs(memoryStream);
                    return memoryStream.ToArray();
                }
            }
        }
    }
}
