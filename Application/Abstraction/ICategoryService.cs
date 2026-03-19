using Application.Common.Result;
using Application.DTOs.Category;

namespace Application.Abstraction
{
    public interface ICategoryService
    {
        Task<Result<IEnumerable<CategoryDto>>> GetAllAsync();
        Task<Result<CategoryDto>> CreateAsync(CreateCategoryDto dto);
        Task<Result<CategoryDto>> UpdateAsync(Guid id, UpdateCategoryDto dto);
        Task<Result> DeleteAsync(Guid id);
    }
}
