// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Invoices\Commands\CreateInvoiceCommand.cs
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Application.Invoices.DTOs;
using MediQueue.Domain.Entities;
using MediQueue.Domain.Exceptions;
using MediQueue.Domain.Interfaces;
using MediQueue.Domain.ValueObjects;

namespace MediQueue.Application.Invoices.Commands;

public class CreateInvoiceItemDto
{
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class CreateInvoiceCommand : ICommand<InvoiceDto>
{
    public Guid PatientId { get; set; }
    public Guid? AppointmentId { get; set; }
    public List<CreateInvoiceItemDto> Items { get; set; } = [];
}

public class CreateInvoiceCommandValidator : AbstractValidator<CreateInvoiceCommand>
{
    public CreateInvoiceCommandValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(items =>
        {
            items.RuleFor(i => i.Description).NotEmpty();
            items.RuleFor(i => i.Quantity).GreaterThan(0);
            items.RuleFor(i => i.UnitPrice).GreaterThanOrEqualTo(0);
        });
    }
}

public class CreateInvoiceCommandHandler : IRequestHandler<CreateInvoiceCommand, Result<InvoiceDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateInvoiceCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<InvoiceDto>> Handle(CreateInvoiceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var patient = await _unitOfWork.Patients.GetByIdAsync(request.PatientId);
            if (patient == null)
            {
                return Result<InvoiceDto>.Failure($"Patient with ID '{request.PatientId}' was not found.");
            }

            var dueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
            var invoice = Invoice.Create(request.PatientId, request.AppointmentId, dueDate);

            foreach (var item in request.Items)
            {
                invoice.AddItem(item.Description, item.Quantity, new Money(item.UnitPrice));
            }

            // Apply default tax if necessary or leave it to another command.
            invoice.ApplyTax(new Money(invoice.SubTotal.Amount * 0.14m)); // 14% VAT

            await _unitOfWork.Invoices.AddAsync(invoice);
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
