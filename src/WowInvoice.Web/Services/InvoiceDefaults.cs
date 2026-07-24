namespace WowInvoice.Web.Services;

public static class InvoiceDefaults
{
    public const int DefaultPaymentTermsDays = 30;

    public static DateTime DefaultDueDate(DateTime issueDate) =>
        issueDate.Date.AddDays(DefaultPaymentTermsDays);
}
