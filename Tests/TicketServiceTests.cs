using Application.DTOs.Ticket;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Abstraction;
using Moq;

namespace HelpDesk.Tests
{
    public class TicketServiceTests
    {
        private readonly Mock<ITicketRepository> _mockTicketRepo;
        private readonly Mock<IRepository<TicketHistory>> _mockHistoryRepo;
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IRepository<Category>> _mockCategory;
        private readonly TicketService _service;

        public TicketServiceTests()
        {
            _mockTicketRepo = new Mock<ITicketRepository>();
            _mockHistoryRepo = new Mock<IRepository<TicketHistory>>();
            _mockCategory = new Mock<IRepository<Category>>();
            _mockUserRepo = new Mock<IUserRepository>();
            _mockMapper = new Mock<IMapper>();

            _service = new TicketService(
                _mockTicketRepo.Object,
                _mockUserRepo.Object,
                _mockHistoryRepo.Object,
                _mockCategory.Object,
                _mockMapper.Object
            );
        }

        [Fact]
        public async Task CreateTicketAsync_ValidData_CreatesTicket()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var dto = new CreateTicketDto { Title = "Test", Description = "Desc", Priority = TicketPriority.Medium, CategoryId = Guid.NewGuid() };

            _mockTicketRepo.Setup(r => r.AddAsync(It.IsAny<Ticket>())).Returns(Task.CompletedTask);

            // Act
            var result = await _service.CreateTicketAsync(clientId, dto);

            // Assert
            Assert.True(result.Success);
            _mockTicketRepo.Verify(r => r.AddAsync(It.Is<Ticket>(t =>
                t.ClientId == clientId &&
                t.Title == dto.Title &&
                t.Status == TicketStatus.New)), Times.Once);
        }

        [Fact]
        public async Task AssignOperatorAsync_TicketNotFound_ReturnsFail()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var operatorId = Guid.NewGuid();

            _mockTicketRepo.Setup(r => r.GetByIdAsync(ticketId)).ReturnsAsync((Ticket?)null);

            // Act
            var result = await _service.AssignOperatorAsync(ticketId, operatorId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Обращение не найдено", result.Error);
        }

        [Fact]
        public async Task AssignOperatorAsync_OperatorNotFound_ReturnsFail()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var operatorId = Guid.NewGuid();
            var ticket = new Ticket { Id = ticketId };

            _mockTicketRepo.Setup(r => r.GetByIdAsync(ticketId)).ReturnsAsync(ticket);
            _mockUserRepo.Setup(r => r.GetByIdAsync(operatorId)).ReturnsAsync((User?)null);

            // Act
            var result = await _service.AssignOperatorAsync(ticketId, operatorId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Оператор не найден", result.Error);
        }

        [Fact]
        public async Task AssignOperatorAsync_NewTicket_UpdatesStatusAndHistory()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var operatorId = Guid.NewGuid();
            var ticket = new Ticket { Id = ticketId, Status = TicketStatus.New };
            var operatorUser = new User { Id = operatorId, FullName = "Ivan Ivanov" };

            _mockTicketRepo.Setup(r => r.GetByIdAsync(ticketId)).ReturnsAsync(ticket);
            _mockUserRepo.Setup(r => r.GetByIdAsync(operatorId)).ReturnsAsync(operatorUser);
            _mockHistoryRepo.Setup(h => h.AddAsync(It.IsAny<TicketHistory>())).Returns(Task.CompletedTask);

            // Act
            var result = await _service.AssignOperatorAsync(ticketId, operatorId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(TicketStatus.InProgress, ticket.Status);
            _mockHistoryRepo.Verify(h => h.AddAsync(It.Is<TicketHistory>(th =>
                th.Action.Contains("Назначен оператор") && th.ChangedById == operatorId)), Times.Once);
            _mockTicketRepo.Verify(r => r.Update(ticket), Times.Once);
        }

        [Fact]
        public async Task ChangeStatusAsync_ClosedStatus_SetsClosedAt()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var ticket = new Ticket { Id = ticketId, Status = TicketStatus.InProgress };

            _mockTicketRepo.Setup(r => r.GetByIdAsync(ticketId)).ReturnsAsync(ticket);
            _mockHistoryRepo.Setup(h => h.AddAsync(It.IsAny<TicketHistory>())).Returns(Task.CompletedTask);

            // Act
            var result = await _service.ChangeStatusAsync(ticketId, TicketStatus.Closed, userId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(TicketStatus.Closed, ticket.Status);
            Assert.NotNull(ticket.ClosedAt);
            _mockHistoryRepo.Verify(h => h.AddAsync(It.IsAny<TicketHistory>()), Times.Once);
        }

        [Fact]
        public async Task GetTicketDetailsAsync_ClientAccessDenied_ReturnsFail()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var clientId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();

            var ticket = new Ticket
            {
                Id = ticketId,
                ClientId = otherUserId
            };

            _mockTicketRepo.Setup(r => r.GetTicketWithDetailsAsync(ticketId)).ReturnsAsync(ticket);

            // Act
            var result = await _service.GetTicketDetailsAsync(ticketId, clientId, UserRole.Client);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Доступ запрещен", result.Error);
        }

        [Fact]
        public async Task GetTicketDetailsAsync_ClientIsOwner_ReturnsSuccess()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var clientId = Guid.NewGuid();

            var ticket = new Ticket
            {
                Id = ticketId,
                ClientId = clientId,
                Comments = new List<Comment>()
            };

            var dto = new TicketDetailsDto { Id = ticketId };

            _mockTicketRepo.Setup(r => r.GetTicketWithDetailsAsync(ticketId)).ReturnsAsync(ticket);
            _mockMapper.Setup(m => m.Map<TicketDetailsDto>(ticket)).Returns(dto);

            // Act
            var result = await _service.GetTicketDetailsAsync(ticketId, clientId, UserRole.Client);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
        }

        [Fact]
        public async Task GetOperatorStatsAsync_ReturnsCorrectCounts()
        {
            // Arrange
            var operatorId = Guid.NewGuid();

            _mockTicketRepo.Setup(r => r.CountByOperatorAsync(operatorId, TicketStatus.InProgress, null, null)).ReturnsAsync(5);
            _mockTicketRepo.Setup(r => r.CountByOperatorAsync(operatorId, TicketStatus.New, null, null)).ReturnsAsync(2);
            _mockTicketRepo.Setup(r => r.CountByOperatorAsync(operatorId, TicketStatus.Closed, null, null)).ReturnsAsync(10);
            _mockTicketRepo.Setup(r => r.CountByOperatorAsync(operatorId, TicketStatus.Closed, It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(3);

            // Act
            var result = await _service.GetOperatorStatsAsync(operatorId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(7, result.Data?.TotalAssigned); // 5 InProgress + 2 New
            Assert.Equal(10, result.Data?.ClosedTotal);
            Assert.Equal(3, result.Data?.ClosedToday);
        }
    }
}