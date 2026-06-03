using FluentAssertions;
using MediQueue.Domain.Entities;
using MediQueue.Domain.Enums;
using MediQueue.Domain.Events;
using MediQueue.Domain.Exceptions;
using MediQueue.Domain.ValueObjects;

namespace MediQueue.UnitTests.Domain;

public class InvoiceTests
{
    private static Invoice CreateDraftInvoice()
    {
        return Invoice.Create(Guid.NewGuid(), null, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)));
    }

    [Fact]
    public void Create_ShouldSetStatusToDraft()
    {
        var invoice = CreateDraftInvoice();

        invoice.Status.Should().Be(InvoiceStatus.Draft);
        invoice.InvoiceNumber.Should().StartWith("INV-");
    }

    [Fact]
    public void Create_ShouldRaiseInvoiceCreatedEvent()
    {
        var invoice = CreateDraftInvoice();

        invoice.DomainEvents.Should().ContainSingle(e => e is InvoiceCreatedEvent);
    }

    [Fact]
    public void AddItem_ShouldIncreaseSubTotalByItemAmount()
    {
        var invoice = CreateDraftInvoice();
        var price = new Money(250);

        invoice.AddItem("Consultation", 1, price);

        invoice.SubTotal.Should().Be(price);
        invoice.Items.Should().HaveCount(1);
    }

    [Fact]
    public void AddItem_WithMultipleItems_ShouldSumCorrectly()
    {
        var invoice = CreateDraftInvoice();

        invoice.AddItem("Consultation", 1, new Money(250));
        invoice.AddItem("X-Ray", 2, new Money(150));

        invoice.SubTotal.Amount.Should().Be(550);
        invoice.Items.Should().HaveCount(2);
    }

    [Fact]
    public void AddItem_WhenNotDraft_ShouldThrowInvalidOperationException()
    {
        var invoice = CreateDraftInvoice();
        invoice.Issue();

        var act = () => invoice.AddItem("Late item", 1, new Money(100));

        act.Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData(1000, 10, 900)]
    [InlineData(500, 20, 400)]
    [InlineData(200, 50, 100)]
    public void ApplyDiscount_ByAmount_ShouldCalculateCorrectTotal(decimal subtotal, decimal discountPct, decimal expectedTotal)
    {
        var invoice = CreateDraftInvoice();
        var itemPrice = new Money(subtotal);
        invoice.AddItem("Service", 1, itemPrice);
        var discountAmount = new Money(subtotal * discountPct / 100);

        invoice.ApplyDiscount(discountAmount);

        invoice.TotalAmount.Amount.Should().Be(expectedTotal);
    }

    [Fact]
    public void ApplyFixedDiscount_ShouldReduceTotalByFixedAmount()
    {
        var invoice = CreateDraftInvoice();
        invoice.AddItem("Service", 1, new Money(1000));

        invoice.ApplyDiscount(new Money(150));

        invoice.TotalAmount.Amount.Should().Be(850);
    }

    [Fact]
    public void ApplyDiscount_WhenExceedsSubtotal_ShouldThrowDomainException()
    {
        var invoice = CreateDraftInvoice();
        invoice.AddItem("Service", 1, new Money(100));

        var act = () => invoice.ApplyDiscount(new Money(200));

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void ApplyTax_ShouldIncreaseTotalByTaxAmount()
    {
        var invoice = CreateDraftInvoice();
        invoice.AddItem("Service", 1, new Money(1000));

        invoice.ApplyTax(new Money(140));

        invoice.TotalAmount.Amount.Should().Be(1140);
    }

    [Fact]
    public void Issue_WhenDraft_ShouldChangeStatusToIssued()
    {
        var invoice = CreateDraftInvoice();

        invoice.Issue();

        invoice.Status.Should().Be(InvoiceStatus.Issued);
    }

    [Fact]
    public void RecordPayment_WhenFullAmount_ShouldMarkAsPaid()
    {
        var invoice = CreateDraftInvoice();
        invoice.AddItem("Service", 1, new Money(500));
        invoice.Issue();

        invoice.RecordPayment(new Money(500), PaymentMethod.Cash);

        invoice.Status.Should().Be(InvoiceStatus.Paid);
        invoice.PaidAmount.Amount.Should().Be(500);
    }

    [Fact]
    public void RecordPayment_WhenFullAmount_ShouldRaiseInvoicePaidEvent()
    {
        var invoice = CreateDraftInvoice();
        invoice.AddItem("Service", 1, new Money(500));
        invoice.Issue();
        invoice.ClearDomainEvents();

        invoice.RecordPayment(new Money(500), PaymentMethod.CreditCard);

        invoice.DomainEvents.Should().Contain(e => e is InvoicePaidEvent);
    }

    [Fact]
    public void RecordPayment_WhenPartialAmount_ShouldMarkAsPartiallyPaid()
    {
        var invoice = CreateDraftInvoice();
        invoice.AddItem("Service", 1, new Money(1000));
        invoice.Issue();

        invoice.RecordPayment(new Money(300), PaymentMethod.Cash);

        invoice.Status.Should().Be(InvoiceStatus.PartiallyPaid);
        invoice.PaidAmount.Amount.Should().Be(300);
    }

    [Fact]
    public void RecordPayment_OnCancelledInvoice_ShouldThrowInvalidOperationException()
    {
        var invoice = CreateDraftInvoice();
        invoice.AddItem("Service", 1, new Money(500));
        invoice.Cancel();

        var act = () => invoice.RecordPayment(new Money(500), PaymentMethod.Cash);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void RecordPayment_OnPaidInvoice_ShouldThrowInvalidOperationException()
    {
        var invoice = CreateDraftInvoice();
        invoice.AddItem("Service", 1, new Money(500));
        invoice.Issue();
        invoice.RecordPayment(new Money(500), PaymentMethod.Cash);

        var act = () => invoice.RecordPayment(new Money(100), PaymentMethod.Cash);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void RecordPayment_ShouldRaisePaymentRecordedEvent()
    {
        var invoice = CreateDraftInvoice();
        invoice.AddItem("Service", 1, new Money(500));
        invoice.Issue();
        invoice.ClearDomainEvents();

        invoice.RecordPayment(new Money(200), PaymentMethod.BankTransfer);

        invoice.DomainEvents.Should().ContainSingle(e => e is PaymentRecordedEvent);
        var evt = invoice.DomainEvents.OfType<PaymentRecordedEvent>().First();
        evt.Method.Should().Be(PaymentMethod.BankTransfer);
    }

    [Fact]
    public void GetRemainingBalance_ShouldReturnCorrectAmount()
    {
        var invoice = CreateDraftInvoice();
        invoice.AddItem("Service", 1, new Money(1000));
        invoice.Issue();

        invoice.RecordPayment(new Money(400), PaymentMethod.Cash);

        invoice.RemainingAmount.Amount.Should().Be(600);
    }

    [Fact]
    public void Cancel_WhenPaid_ShouldThrowInvalidOperationException()
    {
        var invoice = CreateDraftInvoice();
        invoice.AddItem("Service", 1, new Money(500));
        invoice.Issue();
        invoice.RecordPayment(new Money(500), PaymentMethod.Cash);

        var act = () => invoice.Cancel();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Cancel_WhenDraft_ShouldChangeStatusToCancelled()
    {
        var invoice = CreateDraftInvoice();

        invoice.Cancel();

        invoice.Status.Should().Be(InvoiceStatus.Cancelled);
    }

    [Fact]
    public void MarkAsOverdue_WhenIssuedAndPastDueDate_ShouldSetOverdue()
    {
        var invoice = Invoice.Create(Guid.NewGuid(), null, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5)));
        invoice.AddItem("Service", 1, new Money(500));
        invoice.Issue();

        invoice.MarkAsOverdue();

        invoice.Status.Should().Be(InvoiceStatus.Overdue);
    }

    [Fact]
    public void MarkAsOverdue_WhenNotPastDueDate_ShouldThrowInvalidOperationException()
    {
        var invoice = CreateDraftInvoice();
        invoice.AddItem("Service", 1, new Money(500));
        invoice.Issue();

        var act = () => invoice.MarkAsOverdue();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void TotalAmount_WithDiscountAndTax_ShouldComputeCorrectly()
    {
        var invoice = CreateDraftInvoice();
        invoice.AddItem("Service", 2, new Money(500));  // 1000
        invoice.ApplyDiscount(new Money(100));           // -100
        invoice.ApplyTax(new Money(150));                // +150

        invoice.TotalAmount.Amount.Should().Be(1050);
    }
}
