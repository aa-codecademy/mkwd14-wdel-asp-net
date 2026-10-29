namespace Lamazon.ViewModels.DataTable;

public class PagedResultViewModel<T>
{
    public List<T> Items { get; set; } = [];

    public int TotalRecords { get; set; }

    public int TotalDisplayRecords { get; set; }

    public DataTableResponseViewModel<T> ToDataTableResponse(int draw)
    {
        return new DataTableResponseViewModel<T>
        {
            Draw = draw,
            Data = Items,
            RecordsFiltered = TotalDisplayRecords,
            RecordsTotal = TotalRecords
        };
    }
}
