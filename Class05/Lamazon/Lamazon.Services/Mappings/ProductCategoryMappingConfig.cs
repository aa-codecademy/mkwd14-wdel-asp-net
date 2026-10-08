using Lamazon.Domain.Entities;
using Lamazon.ViewModels.Models;
using Mapster;

namespace Lamazon.Services.Mappings;

public class ProductCategoryMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<ProductCategory, ProductCategoryViewModel>();
    }
}
