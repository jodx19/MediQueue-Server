// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Invoices\InvoiceMappingProfile.cs
using AutoMapper;
using MediQueue.Domain.Entities;
using MediQueue.Application.Invoices.DTOs;

namespace MediQueue.Application.Invoices;

public class InvoiceMappingProfile : Profile
{
    public InvoiceMappingProfile()
    {
        CreateMap<Invoice, InvoiceDto>()
            .ForMember(d => d.TotalAmount, opt => opt.MapFrom(s => s.TotalAmount.Amount))
            .ForMember(d => d.PaidAmount, opt => opt.MapFrom(s => s.PaidAmount.Amount))
            .ForMember(d => d.RemainingAmount, opt => opt.MapFrom(s => s.RemainingAmount.Amount));

        CreateMap<Invoice, InvoiceDetailDto>()
            .IncludeBase<Invoice, InvoiceDto>()
            .ForMember(d => d.SubTotal, opt => opt.MapFrom(s => s.SubTotal.Amount))
            .ForMember(d => d.DiscountAmount, opt => opt.MapFrom(s => s.DiscountAmount.Amount))
            .ForMember(d => d.TaxAmount, opt => opt.MapFrom(s => s.TaxAmount.Amount));

        CreateMap<Invoice, InvoiceSummaryDto>()
            .IncludeBase<Invoice, InvoiceDto>();

        CreateMap<InvoiceItem, InvoiceItemDto>()
            .ForMember(d => d.UnitPrice, opt => opt.MapFrom(s => s.UnitPrice.Amount))
            .ForMember(d => d.TotalPrice, opt => opt.MapFrom(s => s.TotalPrice.Amount));

        CreateMap<Payment, PaymentDto>()
            .ForMember(d => d.Amount, opt => opt.MapFrom(s => s.Amount.Amount));
    }
}
