using Application.Common.GenerateReportFile;
using Application.DTOs.Report;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Abstraction;
using Moq;

namespace HelpDesk.Tests
{
    public class ReportServiceTests
    {
        private readonly Mock<ITicketRepository> _mockTicketRepo;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IExcelGenerator> _mockExcelGenerator;
        private readonly Mock<ICsvGenerator> _mockCsvGenerator;
        private readonly ReportService _service;

        public ReportServiceTests()
        {
            _mockTicketRepo = new Mock<ITicketRepository>();
            _mockMapper = new Mock<IMapper>();

            _mockExcelGenerator = new Mock<IExcelGenerator>();
            _mockCsvGenerator = new Mock<ICsvGenerator>();

            _service = new ReportService(
                _mockTicketRepo.Object,
                _mockMapper.Object,
                _mockExcelGenerator.Object,
                _mockCsvGenerator.Object
            );
        }

        [Fact]
        public async Task GetReportAsync_NoDates_ReturnsFail()
        {
            // Arrange
            var filter = new ReportFilterDto { From = default, To = default };

            // Act
            var result = await _service.GetReportAsync(filter);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Необходимо указать период", result.Error);
        }

        [Fact]
        public async Task GetReportAsync_ValidFilter_ReturnsCorrectSummary()
        {
            // Arrange
            var filter = new ReportFilterDto
            {
                From = DateTime.UtcNow.AddDays(-1),
                To = DateTime.UtcNow
            };

            var hardwareCat = new Category { Name = "Hardware" };
            var softwareCat = new Category { Name = "Software" };

            var tickets = new List<Ticket>
            {
                new Ticket
                {
                    Id = Guid.NewGuid(),
                    Status = TicketStatus.Closed,
                    CreatedAt = DateTime.UtcNow.AddHours(-2),
                    ClosedAt = DateTime.UtcNow, // Длительность: 2 часа
                    Category = hardwareCat,
                    Operator = new User { FullName = "Op 1" }
                },
                new Ticket
                {
                    Id = Guid.NewGuid(),
                    Status = TicketStatus.Closed,
                    CreatedAt = DateTime.UtcNow.AddHours(-2),
                    ClosedAt = DateTime.UtcNow, // Длительность: 2 часа
                    Category = hardwareCat,
                    Operator = new User { FullName = "Op 1" }
                },
                new Ticket
                {
                    Id = Guid.NewGuid(),
                    Status = TicketStatus.New,
                    CreatedAt = DateTime.UtcNow,
                    Category = softwareCat
                }
            };

            _mockTicketRepo.Setup(r => r.GetAllWithDetailsAsync(
                It.IsAny<TicketStatus?>(), It.IsAny<Guid?>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), null, It.IsAny<Guid?>()))
                .ReturnsAsync(tickets);

            _mockMapper.Setup(m => m.Map<List<ReportItemDto>>(It.IsAny<List<Ticket>>()))
                       .Returns((List<Ticket> src) =>
                           src.Select(t => new ReportItemDto { TicketId = t.Id }).ToList());

            // Act
            var result = await _service.GetReportAsync(filter);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);

            var summary = result.Data.Summary;

            Assert.Equal(3, summary.TotalTickets);
            Assert.Equal(2, summary.ClosedTickets);
            Assert.Equal(1, summary.OpenTickets);

            // (2 часа + 2 часа) / 2 = 2 часа
            Assert.Equal("0 дн. 2 ч. 0 мин.", summary.AverageResolutionTime);

            Assert.Equal(2, summary.TopCategories.Count);

            var topCategory = summary.TopCategories.First();
            Assert.Equal("Hardware", topCategory.CategoryName);
            Assert.Equal(2, topCategory.Count);
        }

        [Fact]
        public async Task ExportReportAsync_ExcelFormat_CallsExcelGenerator()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var tickets = new List<Ticket>
            {
                new Ticket
                {
                    Id = ticketId,
                    Status = TicketStatus.Closed,
                    CreatedAt = DateTime.UtcNow.AddHours(-2),
                    ClosedAt = DateTime.UtcNow
                }
            };

            var filter = new ReportFilterDto { From = DateTime.UtcNow.AddDays(-1), To = DateTime.UtcNow };

            _mockTicketRepo.Setup(r => r.GetAllWithDetailsAsync(
                It.IsAny<TicketStatus?>(), It.IsAny<Guid?>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), null, It.IsAny<Guid?>()))
                .ReturnsAsync(tickets);

            _mockMapper.Setup(m => m.Map<List<ReportItemDto>>(It.IsAny<List<Ticket>>()))
                .Returns((List<Ticket> src) =>
                    src.Select(t => new ReportItemDto { TicketId = t.Id }).ToList());

            _mockExcelGenerator.Setup(g => g.GenerateExcelWithStats(It.IsAny<ReportSummaryDto>(), It.IsAny<List<ReportItemDto>>()))
                .Returns(new byte[] { 1, 2, 3 });

            // Act
            var result = await _service.ExportReportAsync(filter, "excel");

            // Assert
            Assert.True(result.Success);
            Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", result.Data.ContentType);
            Assert.Contains(".xlsx", result.Data.FileName);

            _mockExcelGenerator.Verify(g => g.GenerateExcelWithStats(It.IsAny<ReportSummaryDto>(), It.IsAny<List<ReportItemDto>>()), Times.Once);
            _mockCsvGenerator.Verify(g => g.GenerateCsv(It.IsAny<List<ReportItemDto>>()), Times.Never);
        }

        [Fact]
        public async Task ExportReportAsync_CsvFormat_CallsCsvGenerator()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var tickets = new List<Ticket> { new Ticket { Id = ticketId } };
            var filter = new ReportFilterDto { From = DateTime.UtcNow.AddDays(-1), To = DateTime.UtcNow };

            _mockTicketRepo.Setup(r => r.GetAllWithDetailsAsync(
                 It.IsAny<TicketStatus?>(), It.IsAny<Guid?>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), null, It.IsAny<Guid?>()))
                 .ReturnsAsync(tickets);

            _mockMapper.Setup(m => m.Map<List<ReportItemDto>>(It.IsAny<List<Ticket>>()))
                .Returns((List<Ticket> src) =>
                    src.Select(t => new ReportItemDto { TicketId = t.Id }).ToList());

            _mockCsvGenerator.Setup(g => g.GenerateCsv(It.IsAny<List<ReportItemDto>>()))
                .Returns(new byte[] { 1 });

            // Act
            var result = await _service.ExportReportAsync(filter, "csv");

            // Assert
            Assert.True(result.Success);
            Assert.Equal("text/csv", result.Data.ContentType);
            Assert.Contains(".csv", result.Data.FileName);

            _mockCsvGenerator.Verify(g => g.GenerateCsv(It.IsAny<List<ReportItemDto>>()), Times.Once);
        }
    }
}