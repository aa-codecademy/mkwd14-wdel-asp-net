namespace Lamazon.ViewModels.DataTable;

/// <summary>
/// The products table also filters by category (the dropdown above the table).
/// </summary>
public class ProductsDataTableRequestViewModel : DataTableRequestViewModel
{
    public int? CategoryId { get; set; }
}
