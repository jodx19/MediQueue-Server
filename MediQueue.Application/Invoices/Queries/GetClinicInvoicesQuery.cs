using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Application.Invoices.DTOs;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.Invoices.Queries;

public record GetClinicInvoicesQuery(
    string? Status,
    DateTime? From,
    DateTime? To,
    int Page = 1,
    int PageSize = 20) : IQuery<PagedResult<InvoiceListItemDto>>;

public class GetClinicInvoicesQueryHandler
    : IRequestHandler<GetClinicInvoicesQuery, Result<PagedResult<InvoiceListItemDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetClinicInvoicesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PagedResult<InvoiceListItemDto>>> Handle(
        GetClinicInvoicesQuery request,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var paged = await _unitOfWork.Invoices.GetPagedAsync(
            request.Status,
            request.From,
            request.To,
            page,
            pageSize,
            cancellationToken);

        var items = paged.Items.Select(i =>
        {
            var doctorName = i.Appointment?.Doctor != null
                ? $"Dr. {i.Appointment.Doctor.PersonName.FullName}"
                : string.Empty;

            return new InvoiceListItemDto
            {
                Id = i.Id,
                InvoiceNumber = i.InvoiceNumber,
                PatientName = i.Patient.PersonName.FullName,
                DoctorName = doctorName,
                TotalAmount = i.TotalAmount.Amount,
                Status = i.Status.ToString(),
                CreatedAt = i.IssuedAt,
            };
        }).ToList();

        var dto = PagedResult<InvoiceListItemDto>.Create(
            items,
            paged.TotalCount,
            paged.PageNumber,
            paged.PageSize);

        return Result<PagedResult<InvoiceListItemDto>>.Success(dto);
    }
}
