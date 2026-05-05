// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Invoices\Commands\CancelInvoiceCommand.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Domain.Exceptions;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.Invoices.Commands;

public record CancelInvoiceCommand(Guid InvoiceId, string Reason = "") : ICommand;

public class CancelInvoiceCommandValidator : AbstractValidator<CancelInvoiceCommand>
{
    public CancelInvoiceCommandValidator()
    {
        RuleFor(x => x.InvoiceId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty();
    }
}

public class CancelInvoiceCommandHandler : IRequestHandler<CancelInvoiceCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public CancelInvoiceCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(CancelInvoiceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var invoice = await _unitOfWork.Invoices.GetByIdAsync(request.InvoiceId);
            if (invoice == null)
            {
                return Result.Failure($"Invoice with ID '{request.InvoiceId}' was not found.");
            }

            invoice.Cancel();

            await _unitOfWork.Invoices.UpdateAsync(invoice);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
