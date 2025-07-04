using MarketBasketAnalysis.Client.Domain.Mining;

namespace MarketBasketAnalysis.Client.Domain.UnitTests;

public class ItemExcluderTests
{
    private readonly ItemExcluder _itemExcluder = new([
        new("item", true, true, true, true),
        new("item2", true, true, true, true),
    ]);

    [Fact]
    public void Ctor_InvalidArguments_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ItemExcluder(null!));
        Assert.Throws<ArgumentException>(() => new ItemExcluder([]));
        Assert.Throws<ArgumentException>(() => new ItemExcluder([null]));
    }

    [Fact]
    public void ShouldExclude_NullItem_ThrowsArgumentNullException()
        => // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _itemExcluder.ShouldExclude(null));

    [Fact]
    public void ShouldExclude_OneOfRulesIsApplicableToItem_ReturnsTrue()
        => // Act & Assert
        Assert.True(_itemExcluder.ShouldExclude(new Item(1, "item", false)));

    [Fact]
    public void ShouldExclude_NoneOfRulesIsApplicableToItem_ReturnsFalse()
        => // Act & Assert
        Assert.False(_itemExcluder.ShouldExclude(new Item(1, "item3", false)));
}