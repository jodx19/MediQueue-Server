// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Invoices\Commands\UpdateInvoiceStatusToOverdueCommand.cs
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.Invoices.Commands;

/// <summary>
/// Background command to scan and update all overdue invoices.
/// </summary>
public record UpdateInvoiceStatusToOverdueCommand : IRequest<Result<int>>;

public class UpdateInvoiceStatusToOverdueCommandHandler : IRequestHandler<UpdateInvoiceStatusToOverdueCommand, Result<int>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateInvoiceStatusToOverdueCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int>> Handle(UpdateInvoiceStatusToOverdueCommand request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var overdueInvoices = await _unitOfWork.Invoices.GetOverdueInvoicesAsync(today);

        if (!overdueInvoices.Any())
        {
            return Result<int>.Success(0);
        }

        int updatedCount = 0;
        foreach (var invoice in overdueInvoices)
        {
            try
            {
                invoice.MarkAsOverdue();
                await _unitOfWork.Invoices.UpdateAsync(invoice);
                updatedCount++;
            }
            catch (Exception)
            {
                // Log error for specific invoice but continue processing others
                continue;
            }
        }

        if (updatedCount > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result<int>.Success(updatedCount);
    }
}
