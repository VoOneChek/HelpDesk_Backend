using Application.DTOs.Faq;

namespace Application.Abstraction
{
    public interface IFaqService
    {
        Task<IEnumerable<FaqDto>> GetAllAsync();
        Task<FaqDto> CreateAsync(CreateFaqDto dto);
    }
}
