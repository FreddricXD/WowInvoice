namespace WowInvoice.Web.Models.Entities;

public interface IUserOwnedEntity
{
    string UserId { get; set; }
    ApplicationUser User { get; set; }
}
