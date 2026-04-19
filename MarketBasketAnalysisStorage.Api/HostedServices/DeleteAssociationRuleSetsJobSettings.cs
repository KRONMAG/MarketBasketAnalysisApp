namespace MarketBasketAnalysisStorage.Api.HostedServices;

public class DeleteAssociationRuleSetsJobSettings
{
    public required int Interval
    {
        get;
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);

            field = value;
        }
    }

    public required int BatchSize
    {
        get;
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);

            field = value;
        }
    }
}
