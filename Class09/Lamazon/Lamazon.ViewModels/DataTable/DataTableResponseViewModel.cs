namespace Lamazon.ViewModels.DataTable;

public class DataTableResponseViewModel<T>
{
    public int Draw { get; set; }

    public int RecordsTotal { get; set; }

    public int RecordsFiltered { get; set; }

    public List<T> Data { get; set; } = [];
}
