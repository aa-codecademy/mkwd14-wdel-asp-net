using Lamazon.ViewModels.DataTable;
using Lamazon.ViewModels.Models;

namespace Lamazon.Services.Abstractions;

public interface IInvoicesService
{
    Task<PagedResultViewModel<InvoiceViewModel>> GetFilteredAsync(DataTableRequestViewModel request, CancellationToken cancellationToken = default);

    Task<InvoiceViewModel> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task SetAsPaidAsync(int id, CancellationToken cancellationToken = default);

    Task CancelAsync(int id, CancellationToken cancellationToken = default);
}
