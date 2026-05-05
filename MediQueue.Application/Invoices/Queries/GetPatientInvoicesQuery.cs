// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Invoices\Queries\GetPatientInvoicesQuery.cs
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Application.Invoices.DTOs;
using MediQueue.Domain.Enums;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.Invoices.Queries;

public record GetPatientInvoicesQuery(Guid PatientId, int PageNumber = 1, int PageSize = 20, InvoiceStatus? Status = null) : IQuery<PagedResult<InvoiceSummaryDto>>;

public class GetPatientInvoicesQueryHandler : IRequestHandler<GetPatientInvoicesQuery, Result<PagedResult<InvoiceSummaryDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetPatientInvoicesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<InvoiceSummaryDto>>> Handle(GetPatientInvoicesQuery request, CancellationToken cancellationToken)
    {
        var pagedInvoices = await _unitOfWork.Invoices.GetByPatientAsync(request.PatientId, request.PageNumber, request.PageSize);
        
        var items = pagedInvoices.Items;
        if (request.Status.HasValue)
        {
            items = items.Where(i => i.Status == request.Status.Value).ToList();
        }

        var itemsDto = items.Select(i => _mapper.Map<InvoiceSummaryDto>(i)).ToList();
        var result = PagedResult<InvoiceSummaryDto>.Create(itemsDto, pagedInvoices.TotalCount, request.PageNumber, request.PageSize);

        return Result<PagedResult<InvoiceSummaryDto>>.Success(result);
    }
}
