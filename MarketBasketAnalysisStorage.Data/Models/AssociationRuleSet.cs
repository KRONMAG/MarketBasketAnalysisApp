namespace MarketBasketAnalysisStorage.Data.Models;

public class AssociationRuleSet
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; } = null!;

    public long TransactionCount { get; set; }
}