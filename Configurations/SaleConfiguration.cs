namespace WebApplication1.Models;

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.HasKey(s => s.IdSale);
        builder.Property(s => s.IdSale).ValueGeneratedOnAdd();
        builder.Property(s => s.CreatedAt).IsRequired();

        builder.HasOne(s => s.Client)
            .WithMany(c => c.Sales)
            .HasForeignKey(s => s.IdClient)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Subscription)
            .WithMany(sub => sub.Sales)
            .HasForeignKey(s => s.IdSubscription)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
