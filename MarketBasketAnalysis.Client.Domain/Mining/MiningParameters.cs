using System;

namespace MarketBasketAnalysis.Client.Domain.Mining
{
    /// <summary>
    /// Represents the parameters used for mining association rules.
    /// </summary>
    public sealed class MiningParameters
    {
        #region Fields and Properties

        /// <summary>
        /// Gets the minimum support threshold for identifying frequent itemsets.
        /// </summary>
        /// <remarks>
        /// The value must be between 0 and 1, where 0 means no support is required, 
        /// and 1 means the itemset must appear in all transactions.
        /// </remarks>
        public double MinSupport { get; }

        /// <summary>
        /// Gets the minimum confidence threshold for generating association rules.
        /// </summary>
        /// <remarks>
        /// The value must be between 0 and 1, where 0 means no confidence is required, 
        /// and 1 means the rule must always hold true.
        /// </remarks>
        public double MinConfidence { get; }

        /// <summary>
        /// Gets the item converter used to group or transform items during the mining process.
        /// </summary>
        /// <remarks>
        /// This is an optional parameter that allows for custom item grouping or transformation logic.
        /// </remarks>
        public IItemConverter ItemConverter { get; }

        /// <summary>
        /// Gets the item excluder used to filter out specific items from the mining process.
        /// </summary>
        /// <remarks>
        /// This is an optional parameter that allows for excluding items based on custom logic.
        /// </remarks>
        public IItemExcluder ItemExcluder { get; }

        /// <summary>
        /// Gets the degree of parallelism to use during the mining process.
        /// </summary>
        /// <remarks>
        /// The value must be between 1 and 512, where higher values allow for more parallel processing.
        /// </remarks>
        public int DegreeOfParallelism { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MiningParameters"/> class.
        /// </summary>
        /// <param name="minSupport">The minimum support threshold for identifying frequent itemsets.</param>
        /// <param name="minConfidence">The minimum confidence threshold for generating association rules.</param>
        /// <param name="itemConverter">An optional item converter for grouping or transforming items.</param>
        /// <param name="itemExcluder">An optional item excluder for filtering out specific items.</param>
        /// <param name="degreeOfParallelism">The degree of parallelism to use during the mining process.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="minSupport"/> or <paramref name="minConfidence"/> is not between 0 and 1,
        /// or if <paramref name="degreeOfParallelism"/> is not between 1 and 512.
        /// </exception>
        public MiningParameters(double minSupport, double minConfidence, IItemConverter itemConverter = null,
            IItemExcluder itemExcluder = null, int degreeOfParallelism = 1)
        {
            if (minSupport < 0 || minSupport > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(minSupport), minSupport,
                    "Minimum support threshold must be between 0 and 1.");
            }

            if (minConfidence < 0 || minConfidence > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(minConfidence), minConfidence,
                    "Minimum confidence threshold must be between 0 and 1.");
            }

            if (degreeOfParallelism < 1 || degreeOfParallelism > 512)
            {
                throw new ArgumentOutOfRangeException(nameof(degreeOfParallelism), degreeOfParallelism,
                    "Degree of parallelism must be between 1 and 512.");
            }

            MinSupport = minSupport;
            MinConfidence = minConfidence;
            ItemConverter = itemConverter;
            ItemExcluder = itemExcluder;
            DegreeOfParallelism = degreeOfParallelism;
        }

        #endregion
    }
}