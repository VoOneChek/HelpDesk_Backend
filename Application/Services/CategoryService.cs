using Application.Abstraction;
using Application.Common.Result;
using Application.DTOs.Category;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Abstraction;

namespace Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IRepository<Category> _repository;
        private readonly IMapper _mapper;

        public CategoryService(IRepository<Category> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<CategoryDto>>> GetAllAsync()
        {
            var categories = await _repository.GetAllAsync();
            return Result<IEnumerable<CategoryDto>>.Ok(_mapper.Map<IEnumerable<CategoryDto>>(categories));
        }

        public async Task<Result<CategoryDto>> CreateAsync(CreateCategoryDto dto)
        {
            var category = _mapper.Map<Category>(dto);
            await _repository.AddAsync(category);
            return Result<CategoryDto>.Ok(_mapper.Map<CategoryDto>(category));
        }

        public async Task<Result<CategoryDto>> UpdateAsync(Guid id, UpdateCategoryDto dto)
        {
            var category = await _repository.GetByIdAsync(id);
            if (category == null) return Result<CategoryDto>.Fail("Категория не найдена");

            _mapper.Map(dto, category);
            await _repository.Update(category);
            return Result<CategoryDto>.Ok(_mapper.Map<CategoryDto>(category));
        }

        public async Task<Result> DeleteAsync(Guid id)
        {
            var category = await _repository.GetByIdAsync(id);
            if (category == null) return Result.Fail("Категория не найдена");

            await _repository.Delete(category);
            return Result.Ok();
        }
    }
}
