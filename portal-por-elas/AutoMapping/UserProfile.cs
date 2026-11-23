using AutoMapper;
using portal_por_elas.Contracts.Request;
using portal_por_elas.Contracts.Response;
using portal_por_elas.Domain.Entities;

namespace portal_por_elas.AutoMapping
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, UserResponseDto>();
            CreateMap<CreateUserDto, User>()
           .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());
        }
    }
}
