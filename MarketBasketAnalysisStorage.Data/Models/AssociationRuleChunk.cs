namespace MarketBasketAnalysisStorage.Data.Models;

public record AssociationRuleChunk(
    long Id,
    IReadOnlyCollection<byte> Data,
    long AssociationRuleSetId);