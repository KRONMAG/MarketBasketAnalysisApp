using MarketBasketAnalysis.Client.Domain.Mining;
using Moq;

namespace MarketBasketAnalysis.Client.Domain.UnitTests;

public class MinerTests
{
    private readonly Miner _miner;
    private readonly Item _itemA;
    private readonly Item _itemB;
    private readonly Item _itemC;
    private readonly IEnumerable<Item[]> _transactions;

    public MinerTests()
    {
        _miner = new Miner();

        _itemA = new(1, "A", false);
        _itemB = new(2, "B", false);
        _itemC = new(3, "C", false);
        
        _transactions =
        [
            new[] { _itemA },
            new[] { _itemA, _itemB, _itemA },
            new[] { _itemA, _itemB },
            new[] { _itemA, _itemC },
            new[] { _itemB, _itemC },
            new[] { _itemA, _itemB, _itemC, _itemB },
        ];
    }

    [Fact]
    public void Mine_InvalidArguments_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _miner.Mine(null, new(0, 0)));
        Assert.Throws<ArgumentNullException>(() => _miner.Mine([], null));
        Assert.Throws<AggregateException>(() => _miner.Mine([null], new(0, 0)));
    }

    [Fact]
    public async Task MineAsync_InvalidArguments_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _miner.MineAsync(null, new(0, 0)));
        await Assert.ThrowsAsync<ArgumentNullException>(() => _miner.MineAsync([], null));
        await Assert.ThrowsAsync<AggregateException>(() => _miner.MineAsync([null], new(0, 0)));
    }

    [Fact]
    public void Mine_WithEmptyTransactions_ReturnsNoAssociationRules()
    {
        // Act
        var actual = _miner.Mine([], new(0, 0));

        // Assert
        Assert.Empty(actual);
    }

    [Fact]
    public void Mine_WithOneItemTransactions_ReturnsNoAssociationRules()
    {
        // Act
        var actual = _miner.Mine([[_itemA]], new(0, 0));

        // Assert
        Assert.Empty(actual);
    }

    [Fact]
    public void Mine_StageChanged_RaisesEvents()
    {
        // Arrange
        var stages = new List<MiningStage>();

        _miner.MiningStageChanged += (_, stage) => stages.Add(stage);

        // Act
        _miner.Mine(_transactions, new(0, 0));

        // Assert
        Assert.Equal(
            [MiningStage.FrequentItemSearch, MiningStage.ItemsetSearch, MiningStage.AssociationRuleGeneration],
            stages);
    }

    [Fact]
    public void Mine_ProgressChanged_RaisesEvents()
    {
        // Arrange
        var progressValues = new List<double>();

        _miner.MiningProgressChanged += (_, progress) => progressValues.Add(progress);

        // Act
        _miner.Mine(GenerateTransactions(), new(0, 0));

        // Assert
        Assert.NotEmpty(progressValues);
        Assert.True(progressValues.All(p => p is >= 0 and <= 100));

        IEnumerable<Item[]> GenerateTransactions()
        {
            foreach (var transaction in _transactions)
            {
                Thread.Sleep(100);

                yield return transaction;
            }
        }
    }

    [Fact]
    public void Mine_WithItemExcluder_ExcludesItems()
    {
        // Arrange
        var itemExcluderMock = new Mock<IItemExcluder>();
        itemExcluderMock
            .Setup(x => x.ShouldExclude(_itemA))
            .Returns(true);
        itemExcluderMock
            .Setup(x => x.ShouldExclude(It.IsIn(_itemB, _itemC)))
            .Returns(false);
        var expected = new List<AssociationRule>()
        {
            new(_itemB, _itemC, 4, 3, 2, 6),
            new(_itemC, _itemB, 3, 4, 2, 6),
        };

        // Act
        var actual = _miner.Mine(_transactions, new(0, 0, itemExcluder: itemExcluderMock.Object));

        // Assert
        AssertEqualAssociationRules(expected, actual);
    }

    [Fact]
    public void Mine_WithItemConverter_ReplacesItems()
    {
        // Arrange
        var group = new Item(4, "Group", true);
        var itemConverterMock = new Mock<IItemConverter>();
        itemConverterMock
            .Setup(x => x.TryConvert(It.IsIn(_itemA, _itemB), out group))
            .Returns(true);
        itemConverterMock
            .Setup(x => x.TryConvert(_itemC, out It.Ref<Item>.IsAny))
            .Returns(false);
        var expected = new List<AssociationRule>()
        {
            new(group, _itemC, 6, 3, 3, 6),
            new(_itemC, group, 3, 6, 3, 6),
        };

        // Act
        var actual = _miner.Mine(_transactions, new(0, 0, itemConverterMock.Object));

        // Assert
        AssertEqualAssociationRules(expected, actual);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(0.5, 0)]
    [InlineData(0, 0.5)]
    [InlineData(0.5, 0.5)]
    public void Mine_WithMinSupportAndMinConfidence_ReturnsRules(
        double minSupport, double minConfidence)
    {
        // Arrange
        var expected = GetAllAssociationRules()
            .Where(r => r.Support >= minSupport && r.Confidence >= minConfidence)
            .ToList();

        // Act
        var actual = _miner.Mine(_transactions, new(minSupport, minConfidence));

        // Assert
        AssertEqualAssociationRules(expected, actual);
    }

    [Fact]
    public void Mine_WithDegreeOfParallelism_ReturnsAllRules()
    {
        // Arrange
        var expected = GetAllAssociationRules();

        // Act
        var actual = _miner.Mine(_transactions, new(0, 0, degreeOfParallelism: expected.Count));

        // Assert
        AssertEqualAssociationRules(expected, actual);
    }

    [Fact]
    public async Task MineAsync_WithAlreadyCanceledToken_ThrowsOperationCancelledException()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        await cancellationTokenSource.CancelAsync();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => _miner.MineAsync(_transactions, new(0, 0), cancellationToken));
    }

    [Fact(Timeout = 100)]
    public async Task MineAsync_CancelTokenDuringMining_ThrowsOperationCancelledException()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => _miner.MineAsync(GetTransactions(), new(0, 0), cancellationToken));

        IEnumerable<Item[]> GetTransactions()
        {
            yield return [_itemA, _itemB];

            cancellationTokenSource.Cancel();

            yield return [_itemB, _itemC];
        }
    }

    private void AssertEqualAssociationRules(IReadOnlyCollection<AssociationRule> expected, IReadOnlyCollection<AssociationRule> actual)
    {
        Assert.Equal(expected.Count, actual.Count);

        var equalityComparer = EqualityComparer<AssociationRule>.Create((x, y) =>
            x!.LeftHandSide.Item.Name == y!.LeftHandSide.Item.Name &&
            x.RightHandSide.Item.Name == y.RightHandSide.Item.Name &&
            x.LeftHandSide.Count == y.LeftHandSide.Count &&
            x.RightHandSide.Count == y.RightHandSide.Count &&
            x.PairCount == y.PairCount &&
            x.TransactionCount == y.TransactionCount);

        Assert.All(expected, e => Assert.Contains(e, actual, equalityComparer));
    }

    private List<AssociationRule> GetAllAssociationRules() =>
        [
            new(_itemA, _itemB, 5, 4, 3, 6),
            new(_itemB, _itemA, 4, 5, 3, 6),
            new(_itemA, _itemC, 5, 3, 2, 6),
            new(_itemC, _itemA, 3, 5, 2, 6),
            new(_itemB, _itemC, 4, 3, 2, 6),
            new(_itemC, _itemB, 3, 4, 2, 6),
        ];
}
