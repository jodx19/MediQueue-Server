// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Invoices\Commands\RecordPaymentCommand.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Application.Invoices.DTOs;
using MediQueue.Domain.Enums;
using MediQueue.Domain.Exceptions;
using MediQueue.Domain.Interfaces;
using MediQueue.Domain.ValueObjects;

namespace MediQueue.Application.Invoices.Commands;

public class RecordPaymentCommand : ICommand<InvoiceDto>
{
    public Guid InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
}

public class RecordPaymentCommandValidator : AbstractValidator<RecordPaymentCommand>
{
    public RecordPaymentCommandValidator()
    {
        RuleFor(x => x.InvoiceId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}

public class RecordPaymentCommandHandler : IRequestHandler<RecordPaymentCommand, Result<InvoiceDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public RecordPaymentCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<InvoiceDto>> Handle(RecordPaymentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var invoice = await _unitOfWork.Invoices.GetByIdAsync(request.InvoiceId);
            if (invoice == null)
            {
                return Result<InvoiceDto>.Failure($"Invoice with ID '{request.InvoiceId}' was not found.");
            }

            // Issue invoice if it's still draft before recording payment
            if (invoice.Status == InvoiceStatus.Draft)
            {
                invoice.Issue();
            }

            invoice.RecordPayment(new Money(request.Amount), request.PaymentMethod, request.ReferenceNumber, request.Notes);

            await _unitOfWork.Invoices.UpdateAsync(invoice);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = _mapper.Map<InvoiceDto>(invoice);
            return Result<InvoiceDto>.Success(dto);
        }
        catch (DomainException ex)
        {
            return Result<InvoiceDto>.Failure(ex.Message);
        }
    }
}
