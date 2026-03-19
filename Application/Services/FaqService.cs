using Application.Abstraction;
using Application.Common.Result;
using Application.DTOs.Category;
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

        public async Task<Result<IEnumerable<FaqDto>>> GetAllAsync()
        {
            var articles = await _repository.GetAllAsync();
            return Result<IEnumerable<FaqDto>>.Ok(_mapper.Map<IEnumerable<FaqDto>>(articles));
        }

        public async Task<Result<FaqDto>> CreateAsync(CreateFaqDto dto)
        {
            var article = _mapper.Map<FaqArticle>(dto);
            article.CreatedAt = DateTime.UtcNow;
            await _repository.AddAsync(article);
            return Result<FaqDto>.Ok(_mapper.Map<FaqDto>(article));
        }

        public async Task<Result<FaqDto>> UpdateAsync(Guid id, CreateFaqDto dto)
        {
            var article = await _repository.GetByIdAsync(id);
            if (article == null) return Result<FaqDto>.Fail("Справка не найдена");

            _mapper.Map(dto, article);
            await _repository.Update(article);
            return Result<FaqDto>.Ok(_mapper.Map<FaqDto>(article));
        }

        public async Task<Result> DeleteAsync(Guid id)
        {
            var article = await _repository.GetByIdAsync(id);
            if (article == null) return Result.Fail("Справка не найдена");

            await _repository.Delete(article);
            return Result.Ok();
        }
    }
}
