namespace MarketBasketAnalysis.Client.Domain.UnitTests;

public class ItemTests
{
    private readonly int _id = 1;
    private readonly string _name = "item";
    private readonly bool _isGroup = true;

    [Fact]
    public void Ctor_InvalidArguments_ThrowsArgumentException() =>
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new Item(_id, null, _isGroup));

    [Fact]
    public void Ctor_CorrectArguments_CreatesItem()
    {
        // Act
        var item = new Item(_id, _name, _isGroup);

        // Assert
        Assert.Equal(_id, item.Id);
        Assert.Equal(_name, item.Name);
        Assert.Equal(_isGroup, item.IsGroup);
    }

    [Fact]
    public void Equals_Null_ReturnsFalse()
    {
        // Arrange
        var item = new Item(_id, _name, _isGroup);

        // Act
        var result = item.Equals(null);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_SameItem_ReturnsTrue()
    {
        // Arrange
        var item = new Item(_id, _name, _isGroup);

        // Act
        // ReSharper disable once EqualExpressionComparison
        var result = item.Equals(item);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_ItemsWithDifferentIds_ReturnsFalse()
    {
        // Arrange
        var item1 = new Item(_id, _name, _isGroup);
        var item2 = new Item(_id + 1, _name, _isGroup);

        // Act
        var result = item1.Equals(item2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_ItemsWithSameIds_ReturnsTrue()
    {
        // Arrange
        var item1 = new Item(_id, _name, _isGroup);
        var item2 = new Item(_id, _name, _isGroup);

        // Act
        var result = item1.Equals(item2);

        // Assert
        Assert.True(result);
    }
}
