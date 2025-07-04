using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MarketBasketAnalysis.Client.Domain.Analysis
{
    internal sealed class TomitaAlgorithm
    {
        #region Nested types

        private sealed class LocalState<TVertex>
        {
            public TVertex[] Clique { get; }

            public HashSet<TVertex> CandidateVertices { get; }

            public HashSet<TVertex> ExcludedVertices { get; }

            private readonly TVertex[] _filteredCandidateVertices;

            private int _filteredCandidateVertexIndex;

            public LocalState(TVertex[] clique, HashSet<TVertex> candidateVertices,
                HashSet<TVertex> excludedVertices, IReadOnlyDictionary<TVertex, HashSet<TVertex>> adjacencyList)
            {
                Clique = clique;
                CandidateVertices = candidateVertices;
                ExcludedVertices = excludedVertices;

                _filteredCandidateVertices =
                    FilterCandidateVerticesByPivot(candidateVertices, excludedVertices, adjacencyList);
            }

            public bool TryGetNextCandidateVertex(out TVertex candidateVertex)
            {
                if (_filteredCandidateVertexIndex == _filteredCandidateVertices.Length)
                {
                    candidateVertex = default;

                    return false;
                }

                candidateVertex = _filteredCandidateVertices[_filteredCandidateVertexIndex++];

                return true;
            }

            private static TVertex[] FilterCandidateVerticesByPivot(HashSet<TVertex> candidateVertices,
                HashSet<TVertex> excludedVertices, IReadOnlyDictionary<TVertex, HashSet<TVertex>> adjacencyList)
            {
                TVertex pivot = default;
                var pivotDegree = 0;
                var isPivotFound = false;

                foreach (var candidatePivot in candidateVertices.Union(excludedVertices))
                {
                    var candidatePivotAdjacentVertices = adjacencyList[candidatePivot];

                    var candidatePivotDegree = candidatePivotAdjacentVertices.Count < candidateVertices.Count
                        ? candidatePivotAdjacentVertices.Count(candidateVertices.Contains)
                        : candidateVertices.Count(candidatePivotAdjacentVertices.Contains);

                    if (candidatePivotDegree > pivotDegree)
                    {
                        pivot = candidatePivot;
                        pivotDegree = candidatePivotDegree;

                        isPivotFound = true;
                    }
                }

                if (!isPivotFound)
                    return candidateVertices.ToArray();

                var pivotAdjacentVertices = adjacencyList[pivot];
                
                return candidateVertices
                    .Where(candidateVertex => !pivotAdjacentVertices.Contains(candidateVertex))
                    .ToArray();
            }
        }

        #endregion

        #region Methods

        public IReadOnlyCollection<MaximalClique<TVertex>> Find<TVertex>(
            IReadOnlyDictionary<TVertex, HashSet<TVertex>> adjacencyList,
            int minCliqueSize, int maxCliqueSize, CancellationToken token = default)
        {
            if (adjacencyList == null)
                throw new ArgumentNullException(nameof(adjacencyList));

            if (adjacencyList.Any(pair => pair.Value == null))
                throw new ArgumentException("Adjacency list cannot contain null values.");
            
            if (minCliqueSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minCliqueSize), minCliqueSize,
                    "Minimum clique size must be greater than zero.");
            }

            if (maxCliqueSize < minCliqueSize)
            {
                throw new ArgumentOutOfRangeException(nameof(maxCliqueSize), maxCliqueSize,
                    "Maximum clique size must be greater than or equal to minimum clique size");
            }
            
            var currentState = new LocalState<TVertex>(Array.Empty<TVertex>(),
                new HashSet<TVertex>(adjacencyList.Keys), new HashSet<TVertex>(), adjacencyList);
            
            var stack = new Stack<LocalState<TVertex>>();
            var maximalCliques = new List<MaximalClique<TVertex>>();

            TVertex candidateVertex;

            bool MoveToNextCandidateVertex()
            {
                if (currentState.TryGetNextCandidateVertex(out candidateVertex))
                    return true;

                while (stack.Count > 0)
                {
                    currentState = stack.Pop();

                    if (currentState.TryGetNextCandidateVertex(out candidateVertex))
                        return true;
                }

                return false;
            }

            while (MoveToNextCandidateVertex())
            {
                token.ThrowIfCancellationRequested();

                var augmentedClique = currentState.Clique.Append(candidateVertex).ToArray();
                var newCandidateVertices = new HashSet<TVertex>(currentState.CandidateVertices);
                var newExcludedVertices = new HashSet<TVertex>(currentState.ExcludedVertices);

                var candidateAdjacentVertices = adjacencyList[candidateVertex];

                newCandidateVertices.IntersectWith(candidateAdjacentVertices);
                newExcludedVertices.IntersectWith(candidateAdjacentVertices);

                currentState.CandidateVertices.Remove(candidateVertex);
                currentState.ExcludedVertices.Add(candidateVertex);

                if (newCandidateVertices.Count == 0 && newExcludedVertices.Count == 0 &&
                    augmentedClique.Length >= minCliqueSize)
                {
                    var maximalClique = new MaximalClique<TVertex>(augmentedClique);

                    maximalCliques.Add(maximalClique);
                }

                if (newCandidateVertices.Count > 0 && augmentedClique.Length < maxCliqueSize)
                {
                    stack.Push(currentState);

                    currentState = new LocalState<TVertex>(augmentedClique, newCandidateVertices,
                        newExcludedVertices, adjacencyList);
                }
            }

            return maximalCliques;
        }

        #endregion
    }
}