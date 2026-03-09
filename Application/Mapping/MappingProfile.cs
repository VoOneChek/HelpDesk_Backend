using Application.DTOs.Category;
using Application.DTOs.Comment;
using Application.DTOs.Faq;
using Application.DTOs.Notification;
using Application.DTOs.Ticket;
using Application.DTOs.TicketHistory;
using Application.DTOs.User;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;

namespace Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserResponseDto>()
                .ForMember(dest => dest.Role,
                    opt => opt.MapFrom(src => src.Role.ToString()));

            CreateMap<UpdateProfileDto, User>();

            CreateMap<RegisterDto, User>()
                .ForMember(dest => dest.Id,
                    opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.PasswordHash,
                    opt => opt.MapFrom(src => BCrypt.Net.BCrypt.HashPassword(src.Password)))
                .ForMember(dest => dest.CreatedAt,
                    opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsBlocked,
                    opt => opt.MapFrom(src => false));


            CreateMap<Category, CategoryDto>();
            CreateMap<CreateCategoryDto, Category>();

            CreateMap<Ticket, TicketResponseDto>()
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.Priority,
                    opt => opt.MapFrom(src => src.Priority.ToString()))
                .ForMember(dest => dest.ClientName,
                    opt => opt.MapFrom(src => src.Client.FullName))
                .ForMember(dest => dest.OperatorName,
                    opt => opt.MapFrom(src => src.Operator != null ? src.Operator.FullName : null))
                .ForMember(dest => dest.CategoryName,
                    opt => opt.MapFrom(src => src.Category.Name));

            CreateMap<Comment, CommentResponseDto>()
                .ForMember(dest => dest.AuthorName,
                    opt => opt.MapFrom(src => src.Author.FullName));

            CreateMap<TicketHistory, TicketHistoryDto>()
                .ForMember(dest => dest.ChangedBy,
                    opt => opt.MapFrom(src => src.ChangedBy.FullName));

            CreateMap<FaqArticle, FaqDto>();
            CreateMap<CreateFaqDto, FaqArticle>();

            CreateMap<Notification, NotificationDto>();
        }
    }
}
