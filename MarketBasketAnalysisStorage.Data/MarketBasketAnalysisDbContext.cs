using MarketBasketAnalysisStorage.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace MarketBasketAnalysisStorage.Data;

public class MarketBasketAnalysisDbContext(DbContextOptions<MarketBasketAnalysisDbContext> options) : DbContext(options)
{
    public DbSet<AssociationRuleSet> AssociationRuleSets { get; init; } = null!;

    public DbSet<AssociationRuleChunk> AssociationRuleChunks { get; init; } = null!;

    public DbSet<ItemChunk> ItemChunks { get; init; } = null!;

    public DbSet<AssociationRuleSetMetadata> AssociationRuleSetMetadatas { get; init; } = null!;
}