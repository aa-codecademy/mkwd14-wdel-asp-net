using Lamazon.Domain.Entities;
using Lamazon.ViewModels.Models;
using Mapster;

namespace Lamazon.Services.Mappings;

public class UserMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<User, UserViewModel>();

        config.NewConfig<RegisterViewModel, User>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.RoleKey)
            .Ignore(dest => dest.PasswordHash);
    }
}
