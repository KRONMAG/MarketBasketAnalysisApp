namespace MarketBasketAnalysisStorage.Data.Models;

public class AssociationRuleSetMetadata
{
    public int Id { get; set; }

    public int? LastSavedItemChunkIndex { get; set; }

    public int? LastItemChunkIndex { get; set; }

    public int? LastSavedRuleChunkIndex { get; set; }

    public int? LastRuleChunkIndex { get; set; }

    public int AssociationRuleSetId { get; set; }

    public bool IsDeleted { get; set; }

    public AssociationRuleSet? AssociationRuleSet { get; set; }
}