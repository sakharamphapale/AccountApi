using AccountApi.Models;

namespace AccountApi.Mappers
{
    public class AutoMapperProfile : AutoMapper.Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<RegistrationRequest, DomainModels.Profile>();

            CreateMap<DomainModels.Profile, DomainModels.Authentication>();
        }
    }
}
