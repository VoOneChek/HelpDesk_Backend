using Application.DTOs.Category;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Abstraction;
using Moq;

namespace HelpDesk.Tests
{
    public class CategoryServiceTests
    {
        private readonly Mock<IRepository<Category>> _mockRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly CategoryService _service;

        public CategoryServiceTests()
        {
            _mockRepository = new Mock<IRepository<Category>>();
            _mockMapper = new Mock<IMapper>();
            _service = new CategoryService(_mockRepository.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsMappedCategories()
        {
            // Arrange
            var categories = new List<Category>
            {
                new Category { Id = Guid.NewGuid(), Name = "Hardware" },
                new Category { Id = Guid.NewGuid(), Name = "Software" }
            }.AsEnumerable();

            var categoryDtos = new List<CategoryDto>
            {
                new CategoryDto { Id = categories.First().Id, Name = "Hardware" }
            };

            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(categories);
            _mockMapper.Setup(m => m.Map<IEnumerable<CategoryDto>>(categories)).Returns(categoryDtos);

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data);
        }

        [Fact]
        public async Task CreateAsync_CallsRepositoryAndReturnsDto()
        {
            // Arrange
            var createDto = new CreateCategoryDto { Name = "Network", Description = "Net issues" };
            var categoryEntity = new Category { Id = Guid.NewGuid(), Name = "Network" };
            var categoryDto = new CategoryDto { Id = categoryEntity.Id, Name = "Network" };

            _mockMapper.Setup(m => m.Map<Category>(createDto)).Returns(categoryEntity);
            _mockMapper.Setup(m => m.Map<CategoryDto>(categoryEntity)).Returns(categoryDto);

            // Act
            var result = await _service.CreateAsync(createDto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Network", result.Data?.Name);
            _mockRepository.Verify(r => r.AddAsync(It.IsAny<Category>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_CategoryNotFound_ReturnsFail()
        {
            // Arrange
            var id = Guid.NewGuid();
            var dto = new UpdateCategoryDto { Name = "Updated" };

            _mockRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Category?)null);

            // Act
            var result = await _service.UpdateAsync(id, dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Категория не найдена", result.Error);
        }

        [Fact]
        public async Task UpdateAsync_CategoryFound_UpdatesAndReturnsDto()
        {
            // Arrange
            var id = Guid.NewGuid();
            var dto = new UpdateCategoryDto { Name = "Updated" };
            var existingCategory = new Category { Id = id, Name = "Old Name" };
            var updatedDto = new CategoryDto { Id = id, Name = "Updated" };

            _mockRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existingCategory);
            _mockMapper.Setup(m => m.Map(dto, existingCategory));
            _mockMapper.Setup(m => m.Map<CategoryDto>(existingCategory)).Returns(updatedDto);

            // Act
            var result = await _service.UpdateAsync(id, dto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Updated", result.Data?.Name);
            _mockRepository.Verify(r => r.Update(existingCategory), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_CategoryNotFound_ReturnsFail()
        {
            // Arrange
            var id = Guid.NewGuid();
            _mockRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Category?)null);

            // Act
            var result = await _service.DeleteAsync(id);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Категория не найдена", result.Error);
        }

        [Fact]
        public async Task DeleteAsync_CategoryFound_DeletesSuccessfully()
        {
            // Arrange
            var id = Guid.NewGuid();
            var category = new Category { Id = id, Name = "To Delete" };

            _mockRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(category);

            // Act
            var result = await _service.DeleteAsync(id);

            // Assert
            Assert.True(result.Success);
            _mockRepository.Verify(r => r.Delete(category), Times.Once);
        }
    }
}