namespace MarketBasketAnalysisStorage.Data.Models;

public record AssociationRuleChunk(
    IReadOnlyCollection<byte> Data,
    int AssociationRuleSetId)
{
    public AssociationRuleChunk(byte[] data, int associationRuleSetId)
        : this((IReadOnlyCollection<byte>)data, associationRuleSetId)
    {

    }
}