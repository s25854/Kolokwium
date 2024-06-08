namespace WebApplication1.Models;

public class SubscriptionsConfiguration : IEntityTypeConfiguration<Subscriptions>
{
    public void Configure(EntityTypeBuilder<Subscriptions> builder)
    {
        builder.HasKey(s => s.IdSubscription);
        builder.Property(s => s.Name).HasMaxLength(100).IsRequired();
        builder.Property(s => s.RenewalPeriod).IsRequired();
        builder.Property(s => s.EndTime).IsRequired();
        builder.Property(s => s.Price).HasColumnType("money").IsRequired();

        builder.HasMany(s => s.Sales)
            .WithOne(s => s.Subscription)
            .HasForeignKey(s => s.IdSubscription);

        builder.HasMany(s => s.Discounts)
            .WithOne(d => d.Subscription)
            .HasForeignKey(d => d.IdSubscription);

        builder.HasMany(s => s.Payments)
            .WithOne(p => p.Subscription)
            .HasForeignKey(p => p.IdSubscription);
    }
}