using Microsoft.EntityFrameworkCore;
using Apex.Domain.Entities;
using Apex.Infrastructure.Persistence.Context;
using Bogus;
using BC = BCrypt.Net.BCrypt;

namespace Apex.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (!context.Users.Any())
        {
            var testUser = new User
            {
                Username = "jho",
                PasswordHash = BC.HashPassword("1318_Http!"),
                Role = "Admin"
            };

            await context.Users.AddAsync(testUser);
            await context.SaveChangesAsync();
            Console.WriteLine("--> Seeded User: jho / 1318_Http!");
        }

        // 1. Only seed if the database is relatively empty
        if (await context.Products.CountAsync() >= 100000) return;

        Console.WriteLine("--> Preparing 100,000 records. This might take a minute...");

        // 2. Performance Hack: Disable change tracking for the bulk insert
        context.ChangeTracker.AutoDetectChangesEnabled = false;

        var faker = new Faker<Product>()
            .CustomInstantiator(f => new Product(
                name: f.Commerce.ProductName(),
                sku: f.Commerce.Ean13(),
                price: f.Random.Decimal(10, 5000),
                stockQuantity: f.Random.Number(0, 1000)
            ));

        // 3. Batching: Insert in groups of 10,000 to prevent timeouts
        for (int i = 0; i < 10; i++)
        {
            var batch = faker.Generate(10000);
            await context.Products.AddRangeAsync(batch);
            await context.SaveChangesAsync();

            Console.WriteLine($"--> [Progress] {(i + 1) * 10000} / 100,000 products seeded.");
        }

        // 4. Reset tracking for normal operation
        context.ChangeTracker.AutoDetectChangesEnabled = true;
        Console.WriteLine("--> Bulk Seeding Finished! Your database is now 'Million-Record' ready.");
    }
}