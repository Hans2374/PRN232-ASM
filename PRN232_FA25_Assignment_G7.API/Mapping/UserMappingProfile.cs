using AutoMapper;
using PRN232_FA25_Assignment_G7.Repositories.Entities;
using PRN232_FA25_Assignment_G7.Services.DTOs.Users;

namespace PRN232_FA25_Assignment_G7.API.Mapping;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        // User → UserResponse
        CreateMap<User, UserResponse>()
            .ConstructUsing(u => new UserResponse(
                u.Id,
                u.Username,
                u.FullName,
                u.Email,
                u.Role.ToString(),
                u.IsActive,
                u.CreatedAt
            ));

        // CreateUserRequest → User
        CreateMap<CreateUserRequest, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => Enum.Parse<Role>(src.Role, true)))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

        // UpdateUserRequest → User (partial update)
        CreateMap<UpdateUserRequest, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Username, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => Enum.Parse<Role>(src.Role, true)))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
    }
}
