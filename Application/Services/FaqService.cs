using Application.Abstraction;
using Application.DTOs.Faq;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Abstraction;

namespace Application.Services
{
    public class FaqService : IFaqService
    {
        private readonly IRepository<FaqArticle> _repository;
        private readonly IMapper _mapper;

        public FaqService(IRepository<FaqArticle> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<FaqDto>> GetAllAsync()
        {
            var articles = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<FaqDto>>(articles);
        }

        public async Task<FaqDto> CreateAsync(CreateFaqDto dto)
        {
            var article = _mapper.Map<FaqArticle>(dto);
            article.Id = Guid.NewGuid();
            article.CreatedAt = DateTime.UtcNow;

            await _repository.AddAsync(article);
            await _repository.SaveChangesAsync();

            return _mapper.Map<FaqDto>(article);
        }
    }
}
