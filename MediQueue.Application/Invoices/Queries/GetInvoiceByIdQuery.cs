// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Invoices\Queries\GetInvoiceByIdQuery.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Application.Invoices.DTOs;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.Invoices.Queries;

public record GetInvoiceByIdQuery(Guid InvoiceId) : IQuery<InvoiceDetailDto>;

public class GetInvoiceByIdQueryHandler : IRequestHandler<GetInvoiceByIdQuery, Result<InvoiceDetailDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetInvoiceByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<InvoiceDetailDto>> Handle(GetInvoiceByIdQuery request, CancellationToken cancellationToken)
    {
        var invoice = await _unitOfWork.Invoices.GetByIdAsync(request.InvoiceId);
        
        if (invoice == null)
        {
            return Result<InvoiceDetailDto>.Failure($"Invoice with ID '{request.InvoiceId}' was not found.");
        }

        var dto = _mapper.Map<InvoiceDetailDto>(invoice);
        return Result<InvoiceDetailDto>.Success(dto);
    }
}
