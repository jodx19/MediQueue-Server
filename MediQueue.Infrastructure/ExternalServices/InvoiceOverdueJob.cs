using MediatR;
using MediQueue.Application.Invoices.Commands;

namespace MediQueue.Infrastructure.ExternalServices;

public class InvoiceOverdueJob
{
    private readonly ISender _sender;

    public InvoiceOverdueJob(ISender sender)
    {
        _sender = sender;
    }

    public async Task ExecuteAsync()
    {
        await _sender.Send(new UpdateInvoiceStatusToOverdueCommand());
    }
}
