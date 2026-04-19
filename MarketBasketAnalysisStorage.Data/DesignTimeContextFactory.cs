using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MarketBasketAnalysisStorage.Data;

// ReSharper disable once UnusedMember.Global
public class DesignTimeContextFactory : IDesignTimeDbContextFactory<MarketBasketAnalysisDbContext>
{
    public virtual MarketBasketAnalysisDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MarketBasketAnalysisDbContext>();

        optionsBuilder.UseNpgsql();

        return new MarketBasketAnalysisDbContext(optionsBuilder.Options);
    }
}