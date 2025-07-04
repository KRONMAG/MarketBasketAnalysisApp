using MarketBasketAnalysis.Client.Domain.Mining;

namespace MarketBasketAnalysis.Client.Domain.UnitTests;

public class ItemExclusionRuleTests
{
    [Fact]
    public void Ctor_InvalidArguments_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ItemExclusionRule(null, true, true, true, true));
        Assert.Throws<ArgumentException>(() => new ItemExclusionRule("item", true, true, false, false));
    }

    [Fact]
    public void ShouldExclude_NullItem_ThrowsArgumentNullException()
    {
        // Arrange
        var rule = new ItemExclusionRule("item", true, true, true, true);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => rule.ShouldExclude(null));
    }

    [Theory]
    [InlineData("item", "item", true, true)]
    [InlineData("item", "item", false, true)]
    [InlineData("item", "ITEM", true, true)]
    [InlineData("item", "ITEM", false, false)]
    public void ShouldExclude_IgnoreCase_WorksCorrectly(
        string itemName, string pattern, bool ignoreCase, bool expected)
    {
        // Arrange
        var item = new Item(1, itemName, false);
        var rule = new ItemExclusionRule(pattern, false, ignoreCase, true, true);

        // Act
        var actual = rule.ShouldExclude(item);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(true, true, true, true)]
    [InlineData(true, false, true, true)]
    [InlineData(true, true, false, false)]
    [InlineData(false, true, true, true)]
    [InlineData(false, false, true, false)]
    [InlineData(false, true, false, true)]
    public void ShouldExclude_ApplyToGroupsAndItems_Matches(
        bool isItemGroup, bool applyToItems, bool applyToGroups, bool expected)
    {
        // Arrange
        var item = new Item(1, "item", isItemGroup);
        var rule = new ItemExclusionRule("item", true, true, applyToItems, applyToGroups);

        // Act
        var actual = rule.ShouldExclude(item);

        // Assert
        Assert.Equal(expected, actual);
    }
}
