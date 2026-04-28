using Dapper;
using Grpc.Core;
using MarketBasketAnalysisStorage.Api.Extensions;
using MarketBasketAnalysisStorage.Contracts.V1;
using MarketBasketAnalysisStorage.Data.Models;
using Npgsql;
using static MarketBasketAnalysisStorage.Contracts.V1.AssociationRuleSetStorage;

namespace MarketBasketAnalysisStorage.Api.Services;

#pragma warning disable CA1062 // Проверить аргументы или открытые методы

public class AssociationRuleSetStorage(
    IConfiguration configuration,
    ILogger<AssociationRuleSetStorage> logger) : AssociationRuleSetStorageBase
{
    public override async Task<GetResponse> Get(GetRequest request, ServerCallContext context)
    {
        try
        {
            var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
            await using var connection = new NpgsqlConnection(connectionString);

            await connection.OpenAsync(context.CancellationToken);

            context.CancellationToken.ThrowIfCancellationRequested();

            var sets = await connection.QueryAsync<AssociationRuleSet>(
                $"""
                SELECT
                    id AS {nameof(AssociationRuleSet.Id)},
                    name AS {nameof(AssociationRuleSet.Name)},
                    description AS {nameof(AssociationRuleSet.Description)},
                    transactions_count AS {nameof(AssociationRuleSet.TransactionsCount)},
                    is_saving_complete AS {nameof(AssociationRuleSet.IsSavingComplete)},
                    is_marked_to_delete AS {nameof(AssociationRuleSet.IsMarkedToDelete)},
                    created_at AS {nameof(AssociationRuleSet.CreatedAt)}
                FROM public.association_rule_sets
                WHERE is_saving_complete = TRUE
                """);

            context.CancellationToken.ThrowIfCancellationRequested();

            return new()
            {
                Sets =
                {
                    sets.Select(static s => new AssociationRuleSetInfo
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Description = s.Description,
                        TransactionsCount = s.TransactionsCount
                    })
                }
            };
        }
        catch (NpgsqlException e)
        {
            const string message = "Unexpected error occurred while loading association rule set info.";

            logger.LogError(e, message);

            throw RpcExceptionHelper.Internal(message);
        }
    }

    //public override async Task<RemoveResponse> Remove(RemoveRequest request, ServerCallContext context)
    //{
    //    if (request.SetId <= 0)
    //    {
    //        RpcExceptionHelper.InvalidArgument("Set id must be positive.");
    //    }

    //    try
    //    {
    //        await dbContext.AssociationRuleSetMetadatas
    //            .Where(m => m.AssociationRuleSetId == request.SetId)
    //            .ExecuteUpdateAsync(
    //                m => m.SetProperty(static p => p.IsDeleted, true),
    //                context.CancellationToken);
    //    }
    //    catch (Exception e) when (e is DbUpdateException or DbException)
    //    {
    //        const string message = "Unexpected error occurred while removing association rule set.";

    //        logger.LogError(e, message);

    //        throw RpcExceptionHelper.Internal(message);
    //    }

    //    return new();
    //}

    //public override async Task Load(LoadRequest request, IServerStreamWriter<LoadResponse> responseStream, ServerCallContext context)
    //{
    //    if (request.SetId <= 0)
    //    {
    //        throw RpcExceptionHelper.InvalidArgument("Set id must be positive.");
    //    }

    //    try
    //    {
    //        var metadata = await dbContext.AssociationRuleSetMetadatas
    //            .AsNoTracking()
    //            .FirstOrDefaultAsync(m => m.AssociationRuleSetId == request.SetId, context.CancellationToken);

    //        if (metadata is null or { IsLoaded: false } or { IsDeleted: true })
    //        {
    //            throw RpcExceptionHelper.NotFound($"Set with id {request.SetId} not found.");
    //        }

    //        var set = await dbContext.AssociationRuleSets
    //            .AsNoTracking()
    //            .FirstOrDefaultAsync(s => s.Id == request.SetId, context.CancellationToken);

    //        await responseStream.WriteAsync(new()
    //        {
    //            Part = new AssociationRuleSetPart()
    //            {
    //                SetInfo = new()
    //                {
    //                    Id = set!.Id,
    //                    Name = set.Name,
    //                    Description = set.Description ?? string.Empty,
    //                    TransactionCount = set.TransactionCount,
    //                }
    //            },
    //        });

    //        var itemChunks = dbContext.ItemChunks
    //            .AsNoTracking()
    //            .Where(c => c.AssociationRuleSetId == request.SetId)
    //            .OrderBy(c => c.Index)
    //            .AsAsyncEnumerable()
    //            .WithCancellation(context.CancellationToken);

    //        await foreach (var itemChunk in itemChunks)
    //        {
    //            var items = new Items().MergeFrom(new MemoryStream())

    //            byte[] a = new ArraySegment<byte>(itemChunk.Data, 0, 100);

    //            await responseStream.WriteAsync(new()
    //            {
    //                Part = new AssociationRuleSetPart()
    //                {
    //                    ItemChunk = new()
    //                    {
    //                        Index = itemChunk.Index,
    //                    }
    //                },
    //            });
    //        }
    //    }
    //    catch
    //    {

    //    }
    //}
}