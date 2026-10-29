using Lamazon.Domain.Entities;
using Lamazon.Domain.Models;
using Lamazon.ViewModels.DataTable;
using Lamazon.ViewModels.Models;
using Mapster;

namespace Lamazon.Services.Mappings;

public class ProductMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Product, ProductViewModel>();
        //.Map(dest => dest.ProductCategoryName, src => src.ProductCategory.Name);

        // The form chooses the category by its id; the id and the status are not the form's business
        config.NewConfig<ProductViewModel, Product>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.ProductCategory)
            .Ignore(dest => dest.ProductStatusId);

        config.NewConfig<PagedResultModel<Product>, PagedResultViewModel<ProductViewModel>>();
    }
}
