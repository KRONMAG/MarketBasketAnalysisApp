using MarketBasketAnalysisStorage.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MarketBasketAnalysisStorage.Api.HostedServices;

public class DeleteAssociationRuleSetsJob(
    IServiceScopeFactory serviceScopeFactory,
    IOptions<DeleteAssociationRuleSetsJobSettings> options,
    ILogger<DeleteAssociationRuleSetsJob> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = serviceScopeFactory.CreateAsyncScope();
                await using var dbContext = scope.ServiceProvider.GetRequiredService<MarketBasketAnalysisDbContext>();

                var metadatas = dbContext
                    .AssociationRuleSetMetadatas
                    .AsNoTracking()
                    .Where(m => m.IsDeleted)
                    .AsAsyncEnumerable()
                    .WithCancellation(stoppingToken);

                await foreach (var metadata in metadatas)
                {
                    await dbContext.ItemChunks
                        .AsNoTracking()
                        .Where(s => s.AssociationRuleSetId == metadata.AssociationRuleSetId)
                        .ExecuteDeleteAsync(stoppingToken);

                    await dbContext.AssociationRuleChunks
                        .AsNoTracking()
                        .Where(s => s.AssociationRuleSetId == metadata.AssociationRuleSetId)
                        .ExecuteDeleteAsync(stoppingToken);

                    await dbContext.AssociationRuleSets
                        .AsNoTracking()
                        .Where(r => r.Id == metadata.AssociationRuleSetId)
                        .ExecuteDeleteAsync(stoppingToken);

                    await dbContext.AssociationRuleSetMetadatas
                        .AsNoTracking()
                        .Where(m => m.AssociationRuleSetId == metadata.AssociationRuleSetId)
                        .ExecuteDeleteAsync(stoppingToken);
                }
            }
            catch(Exception e) when (e is not OperationCanceledException)
            {
                logger.LogError(e, "An error occurred while deleting association rule sets.");
            }

            var interval = TimeSpan.FromMilliseconds(options.Value.Interval);

            await Task.Delay(interval, stoppingToken);
        }
    }
}
