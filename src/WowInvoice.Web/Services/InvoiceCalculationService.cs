using WowInvoice.Web.Models.Entities;

namespace WowInvoice.Web.Services;

public static class InvoiceCalculationService
{
    public static void Recalculate(Invoice invoice)
    {
        var subtotal = invoice.LineItems.Sum(i => Math.Round(i.Quantity * i.UnitPrice, 2));
        var taxAmount = Math.Round(subtotal * (invoice.TaxRate / 100m), 2);
        var grandTotal = Math.Max(0, Math.Round(subtotal + taxAmount - invoice.DiscountAmount, 2));

        invoice.Subtotal = subtotal;
        invoice.TaxAmount = taxAmount;
        invoice.GrandTotal = grandTotal;
        invoice.UpdatedAt = DateTime.UtcNow;
    }

    public static (decimal Subtotal, decimal TaxAmount, decimal GrandTotal) Preview(
        IEnumerable<(int Quantity, decimal UnitPrice)> lines,
        decimal taxRate,
        decimal discountAmount)
    {
        var subtotal = lines.Sum(l => Math.Round(l.Quantity * l.UnitPrice, 2));
        var taxAmount = Math.Round(subtotal * (taxRate / 100m), 2);
        var grandTotal = Math.Max(0, Math.Round(subtotal + taxAmount - discountAmount, 2));
        return (subtotal, taxAmount, grandTotal);
    }
}
