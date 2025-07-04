using System;

namespace MarketBasketAnalysis.Client.Domain.Analysis
{
    /// <summary>
    /// Represents the parameters used for finding maximal cliques in a graph of association rules.
    /// </summary>
    public class MaximalCliqueFindingParameters
    {
        /// <summary>
        /// Gets the minimum size of a clique to be considered during the search.
        /// </summary>
        /// <remarks>
        /// The value must be greater than zero.
        /// </remarks>
        public int MinCliqueSize { get; }

        /// <summary>
        /// Gets the maximum size of a clique to be considered during the search.
        /// </summary>
        /// <remarks>
        /// The value must be greater than or equal to <see cref="MinCliqueSize"/>.
        /// </remarks>
        public int MaxCliqueSize { get; }

        /// <summary>
        /// Gets a value indicating whether one-way links between association rules should be ignored.
        /// </summary>
        /// <remarks>
        /// If set to <c>true</c>, only bidirectional links will be considered when finding cliques.
        /// </remarks>
        public bool IgnoreOneWayLinks { get; }

        /// <summary>
        /// Gets the predicate used to filter association rules before finding maximal cliques.
        /// If <c>null</c>, all rules are considered.
        /// </summary>
        public Predicate<AssociationRule> AssociationRuleFilter { get;}

        /// <summary>
        /// Initializes a new instance of the <see cref="MaximalCliqueFindingParameters"/> class.
        /// </summary>
        /// <param name="minCliqueSize">The minimum size of a clique to be considered.</param>
        /// <param name="maxCliqueSize">The maximum size of a clique to be considered.</param>
        /// <param name="ignoreOneWayLinks">A value indicating whether one-way links should be ignored.</param>
        /// <param name="associationRuleFilter">A predicate used to filter association rules before finding maximal cliques. If <c>null</c>, all rules are considered.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="minCliqueSize"/> is less than or equal to zero, 
        /// or if <paramref name="maxCliqueSize"/> is less than <paramref name="minCliqueSize"/>.
        /// </exception>
        public MaximalCliqueFindingParameters(int minCliqueSize, int maxCliqueSize, bool ignoreOneWayLinks = false,
            Predicate<AssociationRule> associationRuleFilter = null)
        {
            if (minCliqueSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minCliqueSize), minCliqueSize,
                    "Minimum clique size must be greater than zero.");
            }

            if (maxCliqueSize < minCliqueSize)
            {
                throw new ArgumentOutOfRangeException(nameof(maxCliqueSize), maxCliqueSize,
                    "Maximum clique size must be greater than or equal to minimum clique size.");
            }

            MinCliqueSize = minCliqueSize;
            MaxCliqueSize = maxCliqueSize;
            IgnoreOneWayLinks = ignoreOneWayLinks;
            AssociationRuleFilter = associationRuleFilter;
        }
    }
}