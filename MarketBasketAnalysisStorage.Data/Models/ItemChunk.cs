namespace MarketBasketAnalysisStorage.Data.Models;

public record ItemChunk(
    IReadOnlyCollection<byte> Data,
    int AssociationRuleSetId)
{
    public ItemChunk(byte[] data, int associationRuleSetId)
        : this((IReadOnlyCollection<byte>)data, associationRuleSetId)
    {

    }
}