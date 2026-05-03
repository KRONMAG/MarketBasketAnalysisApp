namespace MarketBasketAnalysisStorage.Data.Models;

public record ItemChunk(
    int Id,
    IReadOnlyCollection<byte> Data,
    int AssociationRuleSetId)
{
    public ItemChunk(int id, byte[] data, int associationRuleSetId)
        : this(id, (IReadOnlyCollection<byte>)data, associationRuleSetId)
    {

    }
}