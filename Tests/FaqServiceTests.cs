using Application.DTOs.Faq;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Abstraction;
using Moq;

namespace HelpDesk.Tests
{
    public class FaqServiceTests
    {
        private readonly Mock<IRepository<FaqArticle>> _mockRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly FaqService _service;

        public FaqServiceTests()
        {
            _mockRepository = new Mock<IRepository<FaqArticle>>();
            _mockMapper = new Mock<IMapper>();
            _service = new FaqService(_mockRepository.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsMappedArticles()
        {
            // Arrange
            var articles = new List<FaqArticle>
            {
                new FaqArticle { Id = Guid.NewGuid(), Title = "Question 1" }
            };

            var articleDtos = new List<FaqDto>
            {
                new FaqDto { Title = "Question 1" }
            };

            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(articles);
            _mockMapper.Setup(m => m.Map<IEnumerable<FaqDto>>(articles)).Returns(articleDtos);

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data);
        }

        [Fact]
        public async Task CreateAsync_SetsCreatedAtAndReturnsDto()
        {
            // Arrange
            var createDto = new CreateFaqDto { Title = "New Q", Content = "New A" };
            var articleEntity = new FaqArticle { Id = Guid.NewGuid(), Title = "New Q" };
            var articleDto = new FaqDto { Title = "New Q" };

            _mockMapper.Setup(m => m.Map<FaqArticle>(createDto)).Returns(articleEntity);
            _mockMapper.Setup(m => m.Map<FaqDto>(articleEntity)).Returns(articleDto);

            // Act
            var result = await _service.CreateAsync(createDto);

            // Assert
            Assert.True(result.Success);
            Assert.NotEqual(default(DateTime), articleEntity.CreatedAt);
            _mockRepository.Verify(r => r.AddAsync(articleEntity), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ArticleNotFound_ReturnsFail()
        {
            // Arrange
            var id = Guid.NewGuid();
            _mockRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((FaqArticle?)null);

            // Act
            var result = await _service.UpdateAsync(id, new CreateFaqDto());

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Справка не найдена", result.Error);
        }

        [Fact]
        public async Task UpdateAsync_ArticleFound_UpdatesSuccessfully()
        {
            // Arrange
            var id = Guid.NewGuid();
            var dto = new CreateFaqDto { Title = "Updated Title" };
            var existingArticle = new FaqArticle { Id = id, Title = "Old Title" };
            var updatedDto = new FaqDto { Title = "Updated Title" };

            _mockRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existingArticle);
            _mockMapper.Setup(m => m.Map(dto, existingArticle));
            _mockMapper.Setup(m => m.Map<FaqDto>(existingArticle)).Returns(updatedDto);

            // Act
            var result = await _service.UpdateAsync(id, dto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Updated Title", result.Data?.Title);
            _mockRepository.Verify(r => r.Update(existingArticle), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ArticleNotFound_ReturnsFail()
        {
            // Arrange
            var id = Guid.NewGuid();
            _mockRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((FaqArticle?)null);

            // Act
            var result = await _service.DeleteAsync(id);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Справка не найдена", result.Error);
        }

        [Fact]
        public async Task DeleteAsync_ArticleFound_DeletesSuccessfully()
        {
            // Arrange
            var id = Guid.NewGuid();
            var article = new FaqArticle { Id = id };

            _mockRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(article);

            // Act
            var result = await _service.DeleteAsync(id);

            // Assert
            Assert.True(result.Success);
            _mockRepository.Verify(r => r.Delete(article), Times.Once);
        }
    }
}