namespace MarketBasketAnalysisStorage.Data.Models;

public record AssociationRuleSet(
    long Id,
    string Name,
    string? Description,
    long TransactionsCount,
    bool IsSavingComplete,
    bool IsMarkedToDelete,
    DateTime CreatedAt);