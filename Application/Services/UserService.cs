using Application.Abstraction;
using Application.DTOs.User;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Abstraction;
using System;


namespace Application.Services
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> _repository;
        private readonly IMapper _mapper;

        public UserService(IRepository<User> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<UserResponseDto>> GetAllAsync()
        {
            var users = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<UserResponseDto>>(users);
        }

        public async Task<UserResponseDto> CreateAsync(CreateUserDto dto)
        {
            var user = _mapper.Map<User>(dto);

            user.Id = Guid.NewGuid();
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            user.CreatedAt = DateTime.UtcNow;
            user.IsBlocked = false;

            await _repository.AddAsync(user);
            await _repository.SaveChangesAsync();

            return _mapper.Map<UserResponseDto>(user);
        }

        public async Task BlockUserAsync(Guid userId)
        {
            var user = await _repository.GetByIdAsync(userId)
                       ?? throw new Exception("User not found");

            user.IsBlocked = true;

            _repository.Update(user);
            await _repository.SaveChangesAsync();
        }
    }
}
