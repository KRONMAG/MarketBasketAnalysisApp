namespace MarketBasketAnalysisStorage.Data.Models;

public class AssociationRuleChunk
{
    public int Id { get; set; }

    public IReadOnlyCollection<byte> Data { get; set; } = null!;

    public int AssociationRuleSetId { get; set; }

    public AssociationRuleSet? AssociationRuleSet { get; set; }
}