namespace MarketBasketAnalysisStorage.Data.Models;

public record AssociationRuleChunk(
    int Id,
    IReadOnlyCollection<byte> Data,
    int AssociationRuleSetId)
{
    public AssociationRuleChunk(int id, byte[] data, int associationRuleSetId)
        : this(id, (IReadOnlyCollection<byte>)data, associationRuleSetId)
    {

    }
}