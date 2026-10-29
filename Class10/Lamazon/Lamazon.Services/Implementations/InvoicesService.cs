using Lamazon.DataAccess.Repositories.Abstractions;
using Lamazon.Domain.Entities;
using Lamazon.Domain.Enums;
using Lamazon.Domain.Exceptions;
using Lamazon.Services.Abstractions;
using Lamazon.Services.Extensions;
using Lamazon.ViewModels.DataTable;
using Lamazon.ViewModels.Models;
using MapsterMapper;

namespace Lamazon.Services.Implementations;

public class InvoicesService : IInvoicesService
{
    private readonly IInvoicesRepository _invoicesRepository;
    private readonly IMapper _mapper;

    public InvoicesService(
        IInvoicesRepository invoicesRepository,
        IMapper mapper)
    {
        _invoicesRepository = invoicesRepository;
        _mapper = mapper;
    }

    public async Task<PagedResultViewModel<InvoiceViewModel>> GetFilteredAsync(
        DataTableRequestViewModel request,
        CancellationToken cancellationToken = default)
    {
        var pagedResult = await _invoicesRepository.GetFilteredAsync(request.ToPagedQuery(), cancellationToken);
        var result = _mapper.Map<PagedResultViewModel<InvoiceViewModel>>(pagedResult);
        return result;
    }

    public async Task<InvoiceViewModel> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var invoice = await FindInvoiceAsync(id, cancellationToken);
        return _mapper.Map<InvoiceViewModel>(invoice);
    }

    public async Task SetAsPaidAsync(int id, CancellationToken cancellationToken = default)
    {
        await ChangeStatusAsync(id, InvoiceStatusEnum.Paid, cancellationToken);
    }

    public async Task CancelAsync(int id, CancellationToken cancellationToken = default)
    {
        await ChangeStatusAsync(id, InvoiceStatusEnum.Canceled, cancellationToken);
    }

    private async Task ChangeStatusAsync(
        int id, InvoiceStatusEnum newStatus,
        CancellationToken cancellationToken)
    {
        var invoice = await FindInvoiceAsync(id, cancellationToken);
        if (invoice.InvoiceStatusId != (int)InvoiceStatusEnum.PendingPayment)
        {
            var status = (InvoiceStatusEnum)invoice.InvoiceStatusId;
            throw new BusinessRuleException($"Invoice {invoice.InvoiceNumber} is already {status}.");
        }

        invoice.InvoiceStatusId = (int)newStatus;
        await _invoicesRepository.UpdateAsync(invoice, cancellationToken);
    }


    private async Task<Invoice> FindInvoiceAsync(int id, CancellationToken cancellationToken)
    {
        return await _invoicesRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Invoice), id);
    }
}
