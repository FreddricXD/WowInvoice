using WowInvoice.Web.Models.Entities;
using WowInvoice.Web.Services;

namespace WowInvoice.Tests;

public class InvoiceCalculationServiceTests
{
    [Fact]
    public void Recalculate_ComputesSubtotalTaxAndGrandTotal()
    {
        var invoice = new Invoice
        {
            TaxRate = 10m,
            DiscountAmount = 5m,
            LineItems =
            [
                new InvoiceLineItem { Description = "Item A", Quantity = 2, UnitPrice = 50m },
                new InvoiceLineItem { Description = "Item B", Quantity = 1, UnitPrice = 100m }
            ]
        };

        InvoiceCalculationService.Recalculate(invoice);

        Assert.Equal(200m, invoice.Subtotal);
        Assert.Equal(20m, invoice.TaxAmount);
        Assert.Equal(215m, invoice.GrandTotal);
    }

    [Fact]
    public void Recalculate_DiscountCannotMakeTotalNegative()
    {
        var invoice = new Invoice
        {
            TaxRate = 0m,
            DiscountAmount = 500m,
            LineItems = [new InvoiceLineItem { Description = "Small", Quantity = 1, UnitPrice = 10m }]
        };

        InvoiceCalculationService.Recalculate(invoice);

        Assert.Equal(0m, invoice.GrandTotal);
    }

    [Fact]
    public void Preview_MatchesRecalculate()
    {
        var lines = new[] { (Quantity: 3m, UnitPrice: 25m), (Quantity: 2m, UnitPrice: 10m) };
        var preview = InvoiceCalculationService.Preview(lines, taxRate: 8m, discountAmount: 2m);

        var invoice = new Invoice
        {
            TaxRate = 8m,
            DiscountAmount = 2m,
            LineItems = lines.Select(l => new InvoiceLineItem
            {
                Description = "Test",
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice
            }).ToList()
        };
        InvoiceCalculationService.Recalculate(invoice);

        Assert.Equal(invoice.Subtotal, preview.Subtotal);
        Assert.Equal(invoice.TaxAmount, preview.TaxAmount);
        Assert.Equal(invoice.GrandTotal, preview.GrandTotal);
    }
}
