// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Invoices\Commands\AddInvoiceItemCommand.cs
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

public class AddInvoiceItemCommand : ICommand
{
    public Guid InvoiceId { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class AddInvoiceItemCommandValidator : AbstractValidator<AddInvoiceItemCommand>
{
    public AddInvoiceItemCommandValidator()
    {
        RuleFor(x => x.InvoiceId).NotEmpty();
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0);
    }
}

public class AddInvoiceItemCommandHandler : IRequestHandler<AddInvoiceItemCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public AddInvoiceItemCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(AddInvoiceItemCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var invoice = await _unitOfWork.Invoices.GetByIdAsync(request.InvoiceId);
            if (invoice == null)
            {
                return Result.Failure($"Invoice with ID '{request.InvoiceId}' was not found.");
            }

            invoice.AddItem(request.Description, request.Quantity, new Money(request.UnitPrice));

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
