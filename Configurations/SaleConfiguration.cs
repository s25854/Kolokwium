namespace WebApplication1.Models;

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.HasKey(s => s.IdSale);
        builder.Property(s => s.CreatedAt).IsRequired();

        builder.HasOne(s => s.Client)
            .WithMany(c => c.Sales)
            .HasForeignKey(s => s.IdClient);

        builder.HasOne(s => s.Subscription)
            .WithMany()
            .HasForeignKey(s => s.IdSubscription);
    }
}
