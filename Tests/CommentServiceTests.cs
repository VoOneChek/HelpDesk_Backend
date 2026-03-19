using Application.Abstraction;
using Application.DTOs.Comment;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Abstraction;
using Moq;

namespace HelpDesk.Tests
{
    public class CommentServiceTests
    {
        private readonly Mock<IRepository<Comment>> _mockCommentRepo;
        private readonly Mock<IRepository<Ticket>> _mockTicketRepo;
        private readonly Mock<IRepository<User>> _mockUserRepo;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly Mock<IMapper> _mockMapper;
        private readonly CommentService _service;

        public CommentServiceTests()
        {
            _mockCommentRepo = new Mock<IRepository<Comment>>();
            _mockTicketRepo = new Mock<IRepository<Ticket>>();
            _mockUserRepo = new Mock<IRepository<User>>();
            _mockNotificationService = new Mock<INotificationService>();
            _mockMapper = new Mock<IMapper>();

            _service = new CommentService(
                _mockCommentRepo.Object,
                _mockTicketRepo.Object,
                _mockUserRepo.Object,
                _mockNotificationService.Object,
                _mockMapper.Object
            );
        }

        [Fact]
        public async Task AddCommentAsync_TicketNotFound_ThrowsException()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var authorId = Guid.NewGuid();
            var dto = new CreateCommentDto { Content = "Test" };

            _mockTicketRepo.Setup(r => r.GetByIdAsync(ticketId)).ReturnsAsync((Ticket?)null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.AddCommentAsync(ticketId, authorId, dto));
        }

        [Fact]
        public async Task AddCommentAsync_ClientAddsComment_NotifiesOperator()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var operatorId = Guid.NewGuid();
            var ticketId = Guid.NewGuid();

            var ticket = new Ticket
            {
                Id = ticketId,
                ClientId = clientId,
                OperatorId = operatorId
            };

            var clientUser = new User { Id = clientId, FullName = "Client" };
            var dto = new CreateCommentDto { Content = "Hello from client" };
            var commentDto = new CommentResponseDto { Content = dto.Content };

            _mockTicketRepo.Setup(r => r.GetByIdAsync(ticketId)).ReturnsAsync(ticket);
            _mockUserRepo.Setup(r => r.GetByIdAsync(clientId)).ReturnsAsync(clientUser);
            _mockMapper.Setup(m => m.Map<CommentResponseDto>(It.IsAny<Comment>())).Returns(commentDto);

            // Act
            var result = await _service.AddCommentAsync(ticketId, clientId, dto);

            // Assert
            Assert.True(result.Success);

            _mockNotificationService.Verify(n => n.NotifyAsync(
                operatorId,
                It.Is<string>(s => s.Contains("Новый комментарий"))),
                Times.Once);
        }

        [Fact]
        public async Task AddCommentAsync_OperatorAddsComment_NotifiesClient()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var operatorId = Guid.NewGuid();
            var ticketId = Guid.NewGuid();

            var ticket = new Ticket
            {
                Id = ticketId,
                ClientId = clientId,
                OperatorId = operatorId
            };

            var operatorUser = new User { Id = operatorId, FullName = "Operator" };
            var dto = new CreateCommentDto { Content = "Answer from operator" };
            var commentDto = new CommentResponseDto { Content = dto.Content };

            _mockTicketRepo.Setup(r => r.GetByIdAsync(ticketId)).ReturnsAsync(ticket);
            _mockUserRepo.Setup(r => r.GetByIdAsync(operatorId)).ReturnsAsync(operatorUser);
            _mockMapper.Setup(m => m.Map<CommentResponseDto>(It.IsAny<Comment>())).Returns(commentDto);

            // Act
            // Автор комментария - оператор (operatorId)
            var result = await _service.AddCommentAsync(ticketId, operatorId, dto);

            // Assert
            Assert.True(result.Success);

            _mockNotificationService.Verify(n => n.NotifyAsync(
                clientId,
                It.Is<string>(s => s.Contains("Новый комментарий"))),
                Times.Once);
        }

        [Fact]
        public async Task AddCommentAsync_NoOperatorAssigned_DoesNotNotify()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var ticketId = Guid.NewGuid();

            var ticket = new Ticket
            {
                Id = ticketId,
                ClientId = clientId,
                OperatorId = null
            };

            var clientUser = new User { Id = clientId };
            var dto = new CreateCommentDto { Content = "Help me" };

            _mockTicketRepo.Setup(r => r.GetByIdAsync(ticketId)).ReturnsAsync(ticket);
            _mockUserRepo.Setup(r => r.GetByIdAsync(clientId)).ReturnsAsync(clientUser);

            // Act
            var result = await _service.AddCommentAsync(ticketId, clientId, dto);

            // Assert
            Assert.True(result.Success);
            _mockNotificationService.Verify(n => n.NotifyAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetTicketCommentsAsync_ReturnsFilteredComments()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var otherTicketId = Guid.NewGuid();

            var comments = new List<Comment>
            {
                new Comment { Id = Guid.NewGuid(), TicketId = ticketId, Content = "Comment 1" },
                new Comment { Id = Guid.NewGuid(), TicketId = otherTicketId, Content = "Wrong Comment" },
                new Comment { Id = Guid.NewGuid(), TicketId = ticketId, Content = "Comment 2" }
            };

            _mockCommentRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(comments);

            _mockMapper.Setup(m => m.Map<IEnumerable<CommentResponseDto>>(It.IsAny<IEnumerable<Comment>>()))
                       .Returns((IEnumerable<Comment> source) =>
                           source.Where(c => c.TicketId == ticketId)
                                 .Select(c => new CommentResponseDto { Content = c.Content }));

            // Act
            var result = await _service.GetTicketCommentsAsync(ticketId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count());
            Assert.All(result.Data, dto => Assert.DoesNotContain("Wrong", dto.Content));
        }
    }
}