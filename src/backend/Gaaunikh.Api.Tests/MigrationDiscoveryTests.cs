using Gaaunikh.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Gaaunikh.Api.Tests;

public sealed class MigrationDiscoveryTests
{
    [Fact]
    public void CommerceDbContext_DiscoversInitialMigrationBeforeDependentMigrations()
    {
        var options = new DbContextOptionsBuilder<CommerceDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        using var dbContext = new CommerceDbContext(options);

        var migrations = dbContext.Database.GetMigrations().ToArray();

        Assert.Equal(
            new[] { "202605060001_InitialCommerceFoundation", "20260507035825_InventoryCatalogLedger" },
            migrations);
    }
}
