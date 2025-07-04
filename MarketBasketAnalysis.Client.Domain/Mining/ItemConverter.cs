using System;
using System.Collections.Generic;
using System.Linq;

namespace MarketBasketAnalysis.Client.Domain.Mining
{
    /// <inheritdoc />
    public sealed class ItemConverter : IItemConverter
    {
        #region Fields and Properties

        private readonly Dictionary<Item, ItemConversionRule> _replacementRules;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemConverter"/> class with the specified collection of conversion rules.
        /// </summary>
        /// <param name="conversionRules">
        /// A collection of <see cref="ItemConversionRule"/> objects that define the rules for converting items.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="conversionRules"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="conversionRules"/> is empty or contains <c>null</c> elements or duplicate rules.
        /// </exception>
        public ItemConverter(IReadOnlyCollection<ItemConversionRule> conversionRules)
        {
            if (conversionRules == null)
                throw new ArgumentNullException(nameof(conversionRules));

            if (conversionRules.Count == 0)
            {
                throw new ArgumentException("Collection of item conversion rules cannot be empty.",
                    nameof(conversionRules));
            }

            if (conversionRules.Any(item => item == null))
            {
                throw new ArgumentException("Collection of item conversion rules cannot contain null items.",
                    nameof(conversionRules));
            }

            if (conversionRules.Distinct().Count() != conversionRules.Count)
            {
                throw new ArgumentException("Collection of item conversion rules cannot contain same items.",
                    nameof(conversionRules));
            }

            _replacementRules = conversionRules.ToDictionary(rule => rule.SourceItem);
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public bool TryConvert(Item item, out Item group)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            if (_replacementRules.TryGetValue(item, out var replacementRule))
            {
                group = replacementRule.TargetItem;

                return true;
            }

            group = default;

            return false;
        }

        #endregion
    }
}