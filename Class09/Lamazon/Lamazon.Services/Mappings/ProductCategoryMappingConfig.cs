using Lamazon.Domain.Entities;
using Lamazon.Domain.Models;
using Lamazon.ViewModels.DataTable;
using Lamazon.ViewModels.Models;
using Mapster;

namespace Lamazon.Services.Mappings;

public class ProductCategoryMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<ProductCategory, ProductCategoryViewModel>();

        config.NewConfig<PagedResultModel<ProductCategory>, PagedResultViewModel<ProductCategoryViewModel>>();
    }
}
