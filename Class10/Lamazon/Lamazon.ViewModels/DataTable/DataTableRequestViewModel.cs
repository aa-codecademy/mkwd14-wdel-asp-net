namespace Lamazon.ViewModels.DataTable;

public class DataTableRequestViewModel
{
    /// <summary>A counter DataTables uses to draw the answers in the right order. We send it back unchanged.</summary>
    public int Draw { get; set; }

    /// <summary>Index of the first row of the page (0 = the first row).</summary>
    public int Start { get; set; }

    /// <summary>Rows per page. -1 means "All".</summary>
    public int Length { get; set; } = 10;

    public DataTableSearchViewModel? Search { get; set; }

    public List<DataTableColumnViewModel> Columns { get; set; } = [];

    public List<DataTableOrderViewModel> Order { get; set; } = [];

    /// <summary>The name of the column the table is sorted by, or null.</summary>
    public string? SortColumn
    {
        get
        {
            if (Order.Count == 0)
            {
                return null;
            }

            var columnIndex = Order[0].Column;
            return columnIndex >= 0 && columnIndex < Columns.Count ? Columns[columnIndex].Name : null;
        }
    }

    public bool IsAscending => Order.Count == 0 || !string.Equals(Order[0].Dir, "desc", StringComparison.OrdinalIgnoreCase);
}
