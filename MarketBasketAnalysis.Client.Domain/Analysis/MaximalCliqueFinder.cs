using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MarketBasketAnalysis.Client.Domain.Analysis
{
    /// <inheritdoc />
    public sealed class MaximalCliqueFinder : IMaximalCliqueFinder
    {
        /// <inheritdoc />
        public Task<IReadOnlyCollection<IReadOnlyCollection<AssociationRule>>> FindAsync(
            IReadOnlyCollection<AssociationRule> associationRules, MaximalCliqueFindingParameters parameters,
            CancellationToken token = default)
        {
            ValidateParameters(associationRules, parameters);
            
            return Task.Run((() => FindInternal(associationRules, parameters, token)), token);
        }

        /// <inheritdoc />
        public IReadOnlyCollection<IReadOnlyCollection<AssociationRule>> Find(
            IReadOnlyCollection<AssociationRule> associationRules, MaximalCliqueFindingParameters parameters)
        {
            ValidateParameters(associationRules, parameters);

            return FindInternal(associationRules, parameters, default);
        }

        private static void ValidateParameters(IReadOnlyCollection<AssociationRule> associationRules,
            MaximalCliqueFindingParameters parameters)
        {
            if (associationRules == null)
                throw new ArgumentNullException(nameof(associationRules));

            if (associationRules.Any(item => item == null))
            {
                throw new ArgumentException("Collection of association rules cannot contain null items.",
                    nameof(associationRules));
            }

            if (associationRules.Distinct().Count() != associationRules.Count)
            {
                throw new ArgumentException("Collection of association rules cannot contain same items.",
                    nameof(associationRules));
            }

            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));
        }

        private static (IReadOnlyDictionary<Item, TVertex>, IReadOnlyDictionary<TVertex, Item>) MarkupItems<TVertex>(
            IReadOnlyCollection<AssociationRule> associationRules, Func<TVertex> generateVertex)
        {
            var itemToVertexMap = new Dictionary<Item, TVertex>();
            var vertexToItemMap = new Dictionary<TVertex, Item>();

            foreach (var associationRule in associationRules)
            {
                MarkupItem(associationRule.LeftHandSide);
                MarkupItem(associationRule.RightHandSide);
            }

            return (itemToVertexMap, vertexToItemMap);

            void MarkupItem(AssociationRulePart part)
            {
                var item = part.Item;
                
                if (itemToVertexMap.ContainsKey(item))
                    return;

                var vertex = generateVertex();

                itemToVertexMap.Add(item, vertex);
                vertexToItemMap.Add(vertex, item);
            }
        }

        private static IReadOnlyCollection<IReadOnlyCollection<AssociationRule>> FindInternal(
            IReadOnlyCollection<AssociationRule> associationRules, MaximalCliqueFindingParameters parameters,
            CancellationToken token)
        {
            if (associationRules.Count == 0)
                return Array.Empty<IReadOnlyCollection<AssociationRule>>();

            var itemsCount = associationRules.Select(r => r.LeftHandSide.Item)
                .Union(associationRules.Select(r => r.RightHandSide.Item))
                .Count();

            if (itemsCount <= byte.MaxValue)
            {
                byte vertexCounter = 0;

                return FindInternal(associationRules, parameters, MarkupItems(associationRules, () => vertexCounter++), token);
            }
            // ReSharper disable once RedundantIfElseBlock
            else if (itemsCount <= ushort.MaxValue)
            {
                ushort vertexCounter = 0;

                return FindInternal(associationRules, parameters, MarkupItems(associationRules, () => vertexCounter++), token);
            }
            else
            {
                int vertexCounter = 0;

                return FindInternal(associationRules, parameters, MarkupItems(associationRules, () => vertexCounter++), token);
            }
        }

        private static IReadOnlyCollection<IReadOnlyCollection<AssociationRule>> FindInternal<TVertex>(
            IReadOnlyCollection<AssociationRule> associationRules, MaximalCliqueFindingParameters parameters,
            (IReadOnlyDictionary<Item, TVertex>, IReadOnlyDictionary<TVertex, Item>) maps,
            CancellationToken token)
        {
            var adjacencyList = ConvertToAdjacencyList(associationRules, parameters, maps.Item1, token);

            var maximalCliques = new TomitaAlgorithm()
                .Find(adjacencyList, parameters.MinCliqueSize, parameters.MaxCliqueSize, token);

            var associationRuleSubsets =
                ConvertToAssociationRuleSubsets(maximalCliques, associationRules, maps.Item2, token);

            return associationRuleSubsets;
        }

        private static Dictionary<TVertex, HashSet<TVertex>> ConvertToAdjacencyList<TVertex>(
            IReadOnlyCollection<AssociationRule> associationRules, MaximalCliqueFindingParameters parameters,
            IReadOnlyDictionary<Item, TVertex> itemToVertexMap, CancellationToken token)
        {
            var adjacencyList = new Dictionary<TVertex, HashSet<TVertex>>();

            if (parameters.IgnoreOneWayLinks)
            {
                var candidateEdges = new HashSet<(TVertex, TVertex)>();

                foreach (var associationRule in associationRules)
                {
                    token.ThrowIfCancellationRequested();

                    if (parameters.AssociationRuleFilter != null &&
                        !parameters.AssociationRuleFilter(associationRule))
                    {
                        continue;
                    }

                    var sourceVertex = itemToVertexMap[associationRule.LeftHandSide.Item];
                    var targetVertex = itemToVertexMap[associationRule.RightHandSide.Item];

                    if (candidateEdges.Contains((targetVertex, sourceVertex)))
                    {
                        AddAdjacencyPair(sourceVertex, targetVertex);
                        AddAdjacencyPair(targetVertex, sourceVertex);
                    }
                    else
                    {
                        candidateEdges.Add((sourceVertex, targetVertex));
                    }
                }
            }
            else
            {
                foreach (var associationRule in associationRules)
                {
                    token.ThrowIfCancellationRequested();

                    if (parameters.AssociationRuleFilter != null &&
                        !parameters.AssociationRuleFilter(associationRule))
                    {
                        continue;
                    }

                    var sourceVertex = itemToVertexMap[associationRule.LeftHandSide.Item];
                    var targetVertex = itemToVertexMap[associationRule.RightHandSide.Item];

#pragma warning disable S4158 // Empty collections should not be accessed or iterated
                    if (adjacencyList.TryGetValue(sourceVertex, out var adjacentVertices) &&
                        adjacentVertices.Contains(targetVertex))
                        continue;
#pragma warning restore S4158

                    AddAdjacencyPair(sourceVertex, targetVertex);
                    AddAdjacencyPair(targetVertex, sourceVertex);
                }
            }

            return adjacencyList;

            void AddAdjacencyPair(TVertex vertex, TVertex adjacentVertex)
            {
                if (!adjacencyList.TryGetValue(vertex, out var adjacentVertices))
                {
                    adjacentVertices = new HashSet<TVertex>();

                    adjacencyList.Add(vertex, adjacentVertices);
                }

                adjacentVertices.Add(adjacentVertex);
            }
        }

        private static IReadOnlyCollection<IReadOnlyCollection<AssociationRule>> ConvertToAssociationRuleSubsets<TVertex>(
            IReadOnlyCollection<MaximalClique<TVertex>> maximalCliques, IReadOnlyCollection<AssociationRule> associationRules,
            IReadOnlyDictionary<TVertex, Item> vertexToItemMap, CancellationToken token)
        {
            var associationRuleMap = associationRules.ToDictionary(associationRule =>
                (associationRule.LeftHandSide.Item, associationRule.RightHandSide.Item));

            return maximalCliques
                .Select(maximalClique =>
                {
                    token.ThrowIfCancellationRequested();

                    var associationRuleSubset = new List<AssociationRule>();

                    foreach (var sourceVertex in maximalClique)
                    {
                        var leftHandSide = vertexToItemMap[sourceVertex];

                        foreach (var targetVertex in maximalClique)
                        {
                            var rightHandSide = vertexToItemMap[targetVertex];

                            if (associationRuleMap.TryGetValue((leftHandSide, rightHandSide), out var associationRule))
                                associationRuleSubset.Add(associationRule);
                        }
                    }

                    return associationRuleSubset;
                })
                .ToList();
        }
    }
}
