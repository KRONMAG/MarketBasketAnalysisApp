using MarketBasketAnalysis.Client.Domain.Analysis;

namespace MarketBasketAnalysis.Client.Domain.UnitTests;

public class TomitaAlgorithmTests
{
    private readonly TomitaAlgorithm _algorithm = new();

    [Fact]
    public void Find_AdjacencyListIsNull_ThrowsArgumentNullException() =>
        Assert.Throws<ArgumentNullException>(() =>
            _algorithm.Find((IReadOnlyDictionary<int, HashSet<int>>)null!, 2, 3));

    [Fact]
    public void Find_AdjacencyListHasNullValues_ThrowsArgumentException() =>
        Assert.Throws<ArgumentException>(() =>
            _algorithm.Find(new Dictionary<int, HashSet<int>>() { [1] = null! }, 2, 3));

    [Fact]
    public void Find_MinCliqueSizeIsNegative_ThrowsArgumentOutOfRangeException() =>
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _algorithm.Find(new Dictionary<int, HashSet<int>>(), -1, 3));

    [Fact]
    public void Find_MinCliqueSizeIsGreaterThanMaxCliqueSize_ThrowsArgumentOutOfRangeException() =>
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _algorithm.Find(new Dictionary<int, HashSet<int>>(), 4, 3));

    [Fact]
    public void Find_EmptyGraph_ReturnsEmptyCollection()
    {
        // Arrange
        var graph = new Dictionary<int, HashSet<int>>();
        
        // Act
        var cliques = _algorithm.Find(graph, 1, 10);

        // Assert
        Assert.Empty(cliques);
    }

    [Fact]
    public void Find_GraphWithoutCliquesOfSpecifiedSize_ReturnsEmptyCollection()
    {
        // Arrange
        var graph = new Dictionary<int, HashSet<int>>
        {
            [1] = [2, 4],
            [2] = [1, 3],
            [3] = [2, 4],
            [4] = [1, 3]
        };

        // Act
        var cliques = _algorithm.Find(graph, 3, 4);

        // Assert
        Assert.Empty(cliques);
    }

    [Fact]
    public void Find_CompleteGraph_ReturnsSingleMaximalClique()
    {
        // Arrange
        var graph = new Dictionary<int, HashSet<int>>
        {
            [1] = [2, 3, 4],
            [2] = [1, 3, 4],
            [3] = [1, 2, 4],
            [4] = [1, 2, 3]
        };

        // Act
        var cliques = _algorithm.Find(graph, 1, 4);

        // Assert
        AssertContainsCliques(cliques, [1, 2, 3, 4]);
    }

    [Fact]
    public void Find_GraphWithDisjointCliques_ReturnsAllMaximalCliques()
    {
        // Arrange
        var graph = new Dictionary<int, HashSet<int>>
        {
            [1] = [2, 3],
            [2] = [1, 3],
            [3] = [1, 2],
            [4] = [5],
            [5] = [4]
        };

        // Act
        var cliques = _algorithm.Find(graph, 2, 3);

        // Assert
        AssertContainsCliques(cliques, [1, 2, 3], [4, 5]);
    }

    [Fact]
    public void Find_GraphWithOverlappingCliques_ReturnsAllMaximalCliques()
    {
        // Arrange
        var graph = new Dictionary<int, HashSet<int>>
        {
            [1] = [2, 3],
            [2] = [1, 3, 4],
            [3] = [1, 2, 4],
            [4] = [2, 3]
        };

        // Act
        var cliques = _algorithm.Find(graph, 3, 3).ToList();

        // Assert
        AssertContainsCliques(cliques, [1, 2, 3], [2, 3, 4]);
    }

    [Fact]
    public void Find_CyclicGraph_ReturnsCliquesOfSize2()
    {
        // Arrange
        var graph = new Dictionary<int, HashSet<int>>
        {
            [1] = [2, 4],
            [2] = [1, 3],
            [3] = [2, 4],
            [4] = [1, 3]
        };

        // Act
        var cliques = _algorithm.Find(graph, 2, 2).ToList();

        // Assert
        AssertContainsCliques(cliques, [1, 2], [2, 3], [3, 4], [1, 4]);
    }

    public void Find_CancellationTokenIsActive_ThrowsOperationCanceledException()
    {
        // Arrange
        var cancellationTokenSource = new CancellationTokenSource();

        cancellationTokenSource.Cancel();

        // Act & Assert
        Assert.Throws<OperationCanceledException>(() =>
            _algorithm.Find(new Dictionary<int, HashSet<int>>(), 1, 10, cancellationTokenSource.Token));
    }

    private static void AssertContainsCliques(
        IReadOnlyCollection<IEnumerable<int>> actualCliques,
        params IReadOnlyCollection<int>[] expectedCliques)
    {
        Assert.Equal(expectedCliques.Length, actualCliques.Count);

        foreach (var expectedClique in expectedCliques)
        {
            Assert.Contains(actualCliques, actualClique =>
                actualClique.OrderBy(v => v).SequenceEqual(expectedClique.OrderBy(v => v)));
        }
    }
}
