using WebApplication1.Models;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Context;

public class Context: DbContext
{
    public Context() { }

    public Context(DbContextOptions<Context> options)
        : base(options) { }
    
    public DbSet<Client> Clients { get; set; }
    public DbSet<Discount> Discounts { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Sale> Sales { get; set; }
    public DbSet<Subscription> Subscriptions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(Program).Assembly);
        
    }
}