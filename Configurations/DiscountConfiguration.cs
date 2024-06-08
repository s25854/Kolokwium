namespace WebApplication1.Models;

public class DiscountConfiguration : IEntityTypeConfiguration<Discount>
{
    public void Configure(EntityTypeBuilder<Discount> builder)
    {
        builder.HasKey(d => d.IdDiscount);
        builder.Property(d => d.Value).IsRequired();
        builder.Property(d => d.DateFrom).IsRequired();
        builder.Property(d => d.DateTo).IsRequired();

        builder.HasOne(d => d.Subscription)
            .WithMany(s => s.Discounts)
            .HasForeignKey(d => d.IdSubscription);
    }
}
