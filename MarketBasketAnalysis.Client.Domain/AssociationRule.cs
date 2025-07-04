using System;

namespace MarketBasketAnalysis.Client.Domain
{
    /// <summary>
    /// Represents an association rule - a relationship between two items in market basket analysis,
    /// where the presence of one item (left hand side) implies the presence of another (right hand side).
    /// </summary>
    public sealed class AssociationRule : IEquatable<AssociationRule>
    {
        #region Fields and Properties

        private readonly int _pairCount;
        private readonly int _transactionCount;

        /// <summary>
        /// Gets the left-hand side (LHS) of the association rule.
        /// </summary>
        public AssociationRulePart LeftHandSide { get; }

        /// <summary>
        /// Gets the right-hand side (RHS) of the association rule.
        /// </summary>
        public AssociationRulePart RightHandSide { get; }

        /// <summary>
        /// Gets the number of transactions that contain both the LHS and RHS of the rule.
        /// </summary>
        public int PairCount => _pairCount;

        /// <summary>
        /// Gets the number of transactions.
        /// </summary>
        public int TransactionCount => _transactionCount;

        /// <summary>
        /// Gets the support of the rule, which is the proportion of transactions that contain both the LHS and RHS.
        /// </summary>
        public double Support => (double)_pairCount / _transactionCount;

        /// <summary>
        /// Gets the confidence of the rule, which is the proportion of transactions containing the LHS that also contain the RHS.
        /// </summary>
        public double Confidence => Support / LeftHandSide.Support;

        /// <summary>
        /// Gets the lift of the rule, which measures how much more likely the RHS is to occur given the LHS, compared to its baseline probability.
        /// </summary>
        public double Lift => Confidence / RightHandSide.Support;

        /// <summary>
        /// Gets the conviction of the rule, which measures the strength of the implication in the rule.
        /// </summary>
        public double Conviction => (1 - RightHandSide.Support) / (1 - Confidence);

        /// <summary>
        /// Gets the Yule's Q coefficient, which measures the association between the LHS and RHS.
        /// </summary>
        public double YuleQCoefficient
        {
            get
            {
                var (a, b, c, d) = GetContingencyTable();

                return (a * d - b * c) / (a * d + b * c);
            }
        }

        /// <summary>
        /// Gets the Phi correlation coefficient, which measures the strength of the relationship between the LHS and RHS.
        /// </summary>
        public double PhiCorrelationCoefficient
        {
            get
            {
                var (a, b, c, d) = GetContingencyTable();

                return (a * d - b * c) / Math.Sqrt((a + b) * (a + c) * (b + d) * (c + d));
            }
        }

        /// <summary>
        /// Gets the Chi-squared test statistic, which measures the independence of the LHS and RHS.
        /// </summary>
        public double ChiSquaredTestStatistic
        {
            get
            {
                var (a, b, c, d) = GetContingencyTable();

                return (a + b + c + d) * Math.Pow(a * d - b * c, 2) / ((a + b) * (a + c) * (b + d) * (c + d));
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AssociationRule"/> class.
        /// </summary>
        /// <param name="lhsItem">The item on the left-hand side of the rule.</param>
        /// <param name="rhsItem">The item on the right-hand side of the rule.</param>
        /// <param name="lhsCount">The number of transactions containing the LHS item.</param>
        /// <param name="rhsCount">The number of transactions containing the RHS item.</param>
        /// <param name="pairCount">The number of transactions containing both the LHS and RHS items.</param>
        /// <param name="transactionCount">The total number of transactions.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="lhsItem"/> or <paramref name="rhsItem"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown if the LHS and RHS items are the same.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if any of the counts are invalid (e.g., negative or greater than the total transaction count).
        /// </exception>
        public AssociationRule(Item lhsItem, Item rhsItem, int lhsCount, int rhsCount, int pairCount, int transactionCount)
        {
            ValidateParameters(lhsItem, rhsItem, lhsCount, rhsCount,
                pairCount, transactionCount);

            LeftHandSide = new AssociationRulePart(lhsItem, lhsCount, transactionCount);
            RightHandSide = new AssociationRulePart(rhsItem, rhsCount, transactionCount);

            _pairCount = pairCount;
            _transactionCount = transactionCount;
        }

        #endregion

        #region Methods

        private static void ValidateParameters(Item lhsItem, Item rhsItem, int lhsCount, int rhsCount, int pairCount, int transactionCount)
        {
            if (lhsItem == null)
                throw new ArgumentNullException(nameof(lhsItem));

            if (rhsItem == null)
                throw new ArgumentNullException(nameof(rhsItem));

            if (lhsItem.Equals(rhsItem))
                throw new ArgumentException("Items of left and right hand sides cannot be the the same.");

            if (transactionCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(transactionCount), transactionCount,
                    "Transaction count must be greater than zero.");
            }

            if (pairCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pairCount), pairCount,
                    "Pair count must be greater than zero.");
            }

            if (lhsCount > transactionCount)
            {
                throw new ArgumentOutOfRangeException(nameof(lhsCount), lhsCount,
                    "LHS count cannot be greater than transaction count.");
            }

            if (rhsCount > transactionCount)
            {
                throw new ArgumentOutOfRangeException(nameof(rhsCount), rhsCount,
                    "RHS count cannot be greater than transaction count.");
            }

            if (pairCount > Math.Min(lhsCount, rhsCount))
            {
                throw new ArgumentOutOfRangeException(nameof(pairCount), pairCount,
                    "Pair count cannot be greater than the minimum of LHS count and RHS count.");
            }
        }

        private (double a, double b, double c, double d) GetContingencyTable()
        {
            var a = _pairCount;
            var b = LeftHandSide.Count - _pairCount;
            var c = RightHandSide.Count - _pairCount;
            var d = _transactionCount - a - b - c;

            return (a, b, c, d);
        }

        /// <inheritdoc />
        public bool Equals(AssociationRule other)
        {
            if (ReferenceEquals(null, other))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return LeftHandSide.Equals(other.LeftHandSide) &&
                   RightHandSide.Equals(other.RightHandSide);
        }

        /// <inheritdoc />
        public override int GetHashCode() =>
            LeftHandSide.GetHashCode() * 397 ^ RightHandSide.GetHashCode();

        /// <inheritdoc />
        public override bool Equals(object obj) =>
            Equals(obj as AssociationRule);

        /// <inheritdoc />
        public override string ToString() =>
            $"{LeftHandSide} -> {RightHandSide}";

        #endregion
    }
}
