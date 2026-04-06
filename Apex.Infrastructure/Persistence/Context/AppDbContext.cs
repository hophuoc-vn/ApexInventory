using Apex.Domain.Common;
using Apex.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Apex.Infrastructure.Persistence.Context;

// The Primary Constructor handles the 'options' injection automatically
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();

    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; } = null!;

    public DbSet<SalesStatistic> SalesStatistics { get; set; }

    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        base.OnModelCreating(modelBuilder);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(GetIsDeletedFilter(entityType.ClrType));
            }
        }

        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasPrecision(18, 4);

        modelBuilder.Entity<Order>()
            .Property(o => o.TotalAmount)
            .HasPrecision(18, 4);

        modelBuilder.Entity<OrderItem>()
            .Property(oi => oi.UnitPrice)
            .HasPrecision(18, 4);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasIndex(p => p.Sku).IsUnique();
            entity.Property(p => p.RowVersion).IsRowVersion();
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasIndex(o => o.OrderNumber)
                  .IsUnique();

            entity.HasIndex(o => o.CreatedAt)
                  .HasDatabaseName("IX_Orders_CreatedAt");
        });
    }

    private static LambdaExpression GetIsDeletedFilter(Type type)
    {
        var parameter = Expression.Parameter(type, "e");
        var property = Expression.Property(parameter, nameof(ISoftDelete.IsDeleted));
        var body = Expression.Not(property);
        return Expression.Lambda(body, parameter);
    }
}