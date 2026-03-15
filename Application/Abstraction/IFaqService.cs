using Application.Common.Result;
using Application.DTOs.Faq;

namespace Application.Abstraction
{
    public interface IFaqService
    {
        Task<Result<IEnumerable<FaqDto>>> GetAllAsync();
        Task<Result<FaqDto>> CreateAsync(CreateFaqDto dto);
        Task<Result<FaqDto>> UpdateAsync(Guid id, CreateFaqDto dto);
        Task<Result> DeleteAsync(Guid id);
    }
}
