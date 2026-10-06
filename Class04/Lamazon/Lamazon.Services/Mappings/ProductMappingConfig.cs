using Lamazon.Domain.Entities;
using Lamazon.ViewModels.Models;
using Mapster;

namespace Lamazon.Services.Mappings;

public class ProductMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Product, ProductViewModel>();
            //.Map(dest => dest.ProductCategoryName, src => src.ProductCategory.Name);
    }
}
