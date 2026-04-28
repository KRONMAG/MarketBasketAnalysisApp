namespace MarketBasketAnalysisStorage.Data.Models;

public record ItemChunk(
    long Id,
    IReadOnlyCollection<byte> Data,
    long AssociationRuleSetId);