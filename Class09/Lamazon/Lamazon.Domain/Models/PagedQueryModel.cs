namespace Lamazon.Domain.Models;

public class PagedQueryModel
{
    public int Start { get; set; }

    public int Length { get; set; } = 10;

    public string? SearchValue { get; set; }

    public string? SortColumn { get; set; }

    public bool IsAscending { get; set; } = true;
}
