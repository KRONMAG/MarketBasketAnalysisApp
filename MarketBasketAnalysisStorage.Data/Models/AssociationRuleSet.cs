namespace MarketBasketAnalysisStorage.Data.Models;

public record AssociationRuleSet(
    int Id,
    string Name,
    string? Description,
    int TransactionsCount,
    bool IsSavingComplete,
    bool IsMarkedToDelete,
    DateTime CreatedAt);