namespace Lamazon.Domain.Models;

public class PagedResultModel<T>
{
    public List<T> Items { get; set; } = [];

    public int TotalRecords { get; set; }

    public int TotalDisplayRecords { get; set; }
}
