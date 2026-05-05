// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Invoices\Commands\ApplyDiscountCommand.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Domain.Exceptions;
using MediQueue.Domain.Interfaces;
using MediQueue.Domain.ValueObjects;

namespace MediQueue.Application.Invoices.Commands;

public class ApplyDiscountCommand : ICommand
{
    public Guid InvoiceId { get; set; }
    public decimal DiscountAmount { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class ApplyDiscountCommandValidator : AbstractValidator<ApplyDiscountCommand>
{
    public ApplyDiscountCommandValidator()
    {
        RuleFor(x => x.InvoiceId).NotEmpty();
        RuleFor(x => x.DiscountAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Reason).NotEmpty();
    }
}

public class ApplyDiscountCommandHandler : IRequestHandler<ApplyDiscountCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public ApplyDiscountCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ApplyDiscountCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var invoice = await _unitOfWork.Invoices.GetByIdAsync(request.InvoiceId);
            if (invoice == null)
            {
                return Result.Failure($"Invoice with ID '{request.InvoiceId}' was not found.");
            }

            invoice.ApplyDiscount(new Money(request.DiscountAmount));
            // Notes: Reason is currently not stored in domain entity. Could be added if needed.

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
