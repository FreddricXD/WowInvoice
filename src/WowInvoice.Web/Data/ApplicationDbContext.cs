using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WowInvoice.Web.Models.Entities;

namespace WowInvoice.Web.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceLineItem> InvoiceLineItems => Set<InvoiceLineItem>();
    public DbSet<InvoiceStatusHistory> InvoiceStatusHistories => Set<InvoiceStatusHistory>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(u => u.DisplayName).HasMaxLength(100);
        });

        builder.Entity<Customer>(entity =>
        {
            entity.HasIndex(c => new { c.UserId, c.Email }).IsUnique();
            entity.HasOne(c => c.User)
                .WithMany(u => u.Customers)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Invoice>(entity =>
        {
            entity.HasIndex(i => new { i.UserId, i.InvoiceNumber }).IsUnique();
            entity.HasIndex(i => new { i.UserId, i.Status });
            entity.HasIndex(i => i.DueDate);

            entity.Property(i => i.Subtotal).HasPrecision(18, 2);
            entity.Property(i => i.TaxAmount).HasPrecision(18, 2);
            entity.Property(i => i.GrandTotal).HasPrecision(18, 2);
            entity.Property(i => i.DiscountAmount).HasPrecision(18, 2);
            entity.Property(i => i.TaxRate).HasPrecision(5, 2);

            entity.HasOne(i => i.User)
                .WithMany(u => u.Invoices)
                .HasForeignKey(i => i.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(i => i.Customer)
                .WithMany(c => c.Invoices)
                .HasForeignKey(i => i.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<InvoiceLineItem>(entity =>
        {
            entity.Property(l => l.Quantity).HasPrecision(18, 2);
            entity.Property(l => l.UnitPrice).HasPrecision(18, 2);
        });

        builder.Entity<InvoiceStatusHistory>(entity =>
        {
            entity.HasOne(h => h.Invoice)
                .WithMany(i => i.StatusHistory)
                .HasForeignKey(h => h.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
