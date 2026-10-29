using Lamazon.Domain.Models;
using Lamazon.ViewModels.DataTable;

namespace Lamazon.Services.Extensions;

public static class DataTableRequestExtensions
{
    public static PagedQueryModel ToPagedQuery(this DataTableRequestViewModel request)
    {
        return new PagedQueryModel
        {
            IsAscending = request.IsAscending,
            Length = request.Length,
            SearchValue = request.Search?.Value?.Trim() ?? string.Empty,
            SortColumn = request.SortColumn,
            Start = request.Start,
        };
    }
}
