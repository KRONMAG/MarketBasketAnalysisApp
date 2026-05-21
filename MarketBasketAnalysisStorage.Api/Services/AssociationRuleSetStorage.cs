using Dapper;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MarketBasketAnalysisStorage.Api.Extensions;
using MarketBasketAnalysisStorage.Contracts.V1;
using Npgsql;
using System.Buffers;
using static MarketBasketAnalysisStorage.Contracts.V1.AssociationRuleSetStorage;
using AssociationRuleChunkModel = MarketBasketAnalysisStorage.Data.Models.AssociationRuleChunk;
using AssociationRuleSetGrpc = MarketBasketAnalysisStorage.Contracts.V1.AssociationRuleSet;
using ItemChunkGrpc = MarketBasketAnalysisStorage.Contracts.V1.ItemChunk;
using AssociationRuleSetModel = MarketBasketAnalysisStorage.Data.Models.AssociationRuleSet;
using ItemChunkModel = MarketBasketAnalysisStorage.Data.Models.ItemChunk;
using AssociationRuleChunkGrpc = MarketBasketAnalysisStorage.Contracts.V1.AssociationRuleChunk;

namespace MarketBasketAnalysisStorage.Api.Services;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1062:Validate arguments of public methods")]
public class AssociationRuleSetStorage(ILogger<AssociationRuleSetStorage> logger) : AssociationRuleSetStorageBase
{
    public override async Task<GetResponse> Get(GetRequest request, ServerCallContext context)
    {
        var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
        await using var connection = new NpgsqlConnection(connectionString);

        await connection.OpenAsync(context.CancellationToken);

        var command = new CommandDefinition(
            """
            SELECT *
            FROM association_rule_sets
            WHERE is_saving_complete = TRUE
                AND is_marked_to_delete = FALSE
            """,
            cancellationToken: context.CancellationToken);
        IEnumerable<AssociationRuleSetModel> models;
        
        try
        {
            models = await connection.QueryAsync<AssociationRuleSetModel>(command);
        }
        catch (NpgsqlException e)
        {
            const string message = "Unexpected error occurred while loading association rule sets.";

            logger.LogError(e, message);

            throw RpcExceptionHelper.Internal(message);
        }
        
        return new()
        {
            AssociationRuleSets =
            {
                models.Select(static model =>
                {
                    var grpc = new AssociationRuleSetGrpc
                    {
                        Id = model.Id,
                        Name = model.Name,
                        TransactionsCount = model.TransactionsCount,
                        CreatedAt = model.CreatedAt.ToTimestamp(),
                    };

                    if (model.Description != null)
                    {
                        grpc.Description = model.Description;
                    }

                    return grpc;
                })
            }
        };
    }

    public override async Task<RemoveResponse> Remove(RemoveRequest request, ServerCallContext context)
    {
        if (request.AssociationRuleSetId <= 0)
        {
            throw RpcExceptionHelper.InvalidArgument("Association rule set id must be positive.");
        }

        var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
        await using var connection = new NpgsqlConnection(connectionString);

        await connection.OpenAsync(context.CancellationToken);

        var command = new CommandDefinition(
            """
            UPDATE association_rule_sets
            SET is_marked_to_delete = TRUE
            WHERE id = @Id
                AND is_saving_complete = TRUE
                AND is_marked_to_delete = FALSE
            """,
            parameters: new { Id = request.AssociationRuleSetId },
            cancellationToken: context.CancellationToken);

        try
        {
            await connection.ExecuteAsync(command);
        }
        catch (NpgsqlException e)
        {
            const string message = "Unexpected error occurred while removing association rule set.";

            logger.LogError(e, message);

            throw RpcExceptionHelper.Internal(message);
        }

        return new();
    }

    public override async Task<StartSaveResponse> StartSave(StartSaveRequest request, ServerCallContext context)
    {
        var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

        await using var connection = new NpgsqlConnection(connectionString);

        await connection.OpenAsync(context.CancellationToken);

        var model = new AssociationRuleSetModel(
            0,
            request.AssociationRuleSet.Name,
            request.AssociationRuleSet.HasDescription
                ? request.AssociationRuleSet.Description
                : null,
            request.AssociationRuleSet.TransactionsCount,
            false,
            false,
            DateTime.UtcNow);

        var command = new CommandDefinition(
            $"""
             INSERT INTO association_rule_sets (
                 name,
                 description,
                 transactions_count,
                 is_saving_complete,
                 is_marked_to_delete,
                 created_at)
             VALUES (
                 @{nameof(AssociationRuleSetModel.Name)},
                 @{nameof(AssociationRuleSetModel.Description)},
                 @{nameof(AssociationRuleSetModel.TransactionsCount)},
                 @{nameof(AssociationRuleSetModel.IsSavingComplete)},
                 @{nameof(AssociationRuleSetModel.IsMarkedToDelete)},
                 @{nameof(AssociationRuleSetGrpc.CreatedAt)})
             RETURNING id;
             """,
            parameters: model,
            cancellationToken: context.CancellationToken);
        var modelId = await connection.ExecuteScalarAsync<int>(command);

        return new() { AssociationRuleSetId = modelId };
    }

    public override async Task<SaveItemChunkResponse> SaveItemChunk(
        SaveItemChunkRequest request,
        ServerCallContext context)
    {
        if (request.AssociationRuleSetId <= 0)
        {
            throw RpcExceptionHelper.InvalidArgument("Association rule set id must be positive.");
        }

        if (request.ItemChunk.Items.Count == 0)
        {
            throw RpcExceptionHelper.InvalidArgument("Item chunk must contain at least one item.");
        }

        byte[]? buffer = null;
        var chunkSize = request.ItemChunk.CalculateSize();

        try
        {
            buffer = ArrayPool<byte>.Shared.Rent(chunkSize);

            var bufferWrapper = new ArraySegment<byte>(buffer, 0, chunkSize);

            request.ItemChunk.WriteTo(bufferWrapper);

            var itemChunk = new ItemChunkModel(bufferWrapper, request.AssociationRuleSetId);

            var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

            await using var connection = new NpgsqlConnection(connectionString);

            await connection.OpenAsync(context.CancellationToken);

            var command = new CommandDefinition(
                $"""
                 BEGIN;
                 SELECT pg_advisory_xact_lock_shared(@{nameof(ItemChunkModel.AssociationRuleSetId)});
                 INSERT INTO item_chunks (data, association_rule_set_id)
                 SELECT @{nameof(ItemChunkModel.Data)}, @{nameof(ItemChunkModel.AssociationRuleSetId)}
                 WHERE EXISTS(
                     SELECT 1
                     FROM association_rule_sets
                     WHERE id = @{nameof(ItemChunkModel.AssociationRuleSetId)}
                        AND is_saving_complete = FALSE);
                 END;
                 """,
                parameters: itemChunk,
                cancellationToken: context.CancellationToken);

            var rowsAffected = await connection.ExecuteAsync(command);

            if (rowsAffected == 0)
            {
                throw RpcExceptionHelper.FailedPrecondition(
                    $"Association rule set with ID {request.AssociationRuleSetId} not found or its saving is completed.");
            }

            return new();
        }
        catch(NpgsqlException e)
        {
            const string message = "Unexpected error occurred while saving item chunk.";

            logger.LogError(e, message);

            throw RpcExceptionHelper.Internal(message);
        }
        finally
        {
            if (buffer != null)
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
    }

    public override async Task<SaveAssociationRuleChunkResponse> SaveAssociationRuleChunk(
        SaveAssociationRuleChunkRequest request,
        ServerCallContext context)
    {
        if (request.AssociationRuleSetId <= 0)
        {
            throw RpcExceptionHelper.InvalidArgument("Association rule set id must be positive.");
        }

        if (request.AssociationRuleChunk.AssociationRules.Count == 0)
        {
            throw RpcExceptionHelper.InvalidArgument("Association rule chunk must contain at least one item");
        }

        byte[]? buffer = null;

        try
        {
            var chunkSize = request.AssociationRuleChunk.CalculateSize();

            buffer = ArrayPool<byte>.Shared.Rent(chunkSize);

            var bufferWrapper = new ArraySegment<byte>(buffer, 0, chunkSize);

            request.AssociationRuleChunk.WriteTo(bufferWrapper);

            var model = new AssociationRuleChunkModel(bufferWrapper, request.AssociationRuleSetId);

            var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

            await using var connection = new NpgsqlConnection(connectionString);

            await connection.OpenAsync(context.CancellationToken);

            var command = new CommandDefinition(
                $"""
                 BEGIN;
                 SELECT pg_advisory_xact_lock_shared(@{nameof(AssociationRuleChunkModel.AssociationRuleSetId)});
                 INSERT INTO association_rule_chunks (data, association_rule_set_id)
                 SELECT
                    @{nameof(AssociationRuleChunkModel.Data)},
                    @{nameof(AssociationRuleChunkModel.AssociationRuleSetId)}
                 WHERE EXISTS(
                     SELECT 1
                     FROM association_rule_sets
                     WHERE id = @{nameof(AssociationRuleChunkModel.AssociationRuleSetId)}
                        AND is_saving_complete = FALSE);
                 END;
                 """,
                parameters: model,
                cancellationToken: context.CancellationToken);
            var rowsAffected = await connection.ExecuteAsync(command);

            if (rowsAffected == 0)
            {
                throw RpcExceptionHelper.FailedPrecondition(
                    $"Association rule set with id {request.AssociationRuleSetId} not found or its saving is completed.");
            }

            return new();
        }
        catch (NpgsqlException e)
        {
            const string message = "Unexpected error occurred while saving association rule chunk.";

            logger.LogError(e, message);

            throw RpcExceptionHelper.Internal(message);
        }
        finally
        {
            if (buffer != null)
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
    }

    public override async Task<CompleteSaveResponse> CompleteSave(CompleteSaveRequest request, ServerCallContext context)
    {
        if (request.AssociationRuleSetId <= 0)
        {
            throw RpcExceptionHelper.InvalidArgument("Association rule set id must be positive.");
        }

        try
        {
            var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

            await using var connection = new NpgsqlConnection(connectionString);

            await connection.OpenAsync(context.CancellationToken);

            var command = new CommandDefinition(
                """
                BEGIN;
                SELECT pg_advisory_xact_lock(@Id);
                UPDATE association_rule_sets
                SET is_saving_complete = TRUE
                WHERE id = @Id
                    AND is_saving_complete = FALSE
                    AND EXISTS (SELECT 1 FROM item_chunks WHERE association_rule_set_id = @Id)
                    AND EXISTS (SELECT 1 FROM association_rule_chunks WHERE association_rule_set_id = @Id);
                COMMIT;
                """,
                new { Id = request.AssociationRuleSetId },
                cancellationToken: context.CancellationToken);
            var rowsAffected = await connection.ExecuteAsync(command);

            if (rowsAffected == 0)
            {
                throw RpcExceptionHelper.FailedPrecondition(
                    $"""
                     Failed to complete saving association rule set with id {request.AssociationRuleSetId}.
                     Possible reasons:
                     - set not found;
                     - set already saved;
                     - set is empty: it has no saved item or association rule chunks.
                     """);
            }

            return new();
        }
        catch (NpgsqlException e)
        {
            const string message = "Unexpected error occurred while completing saving of association rule set.";

            logger.LogError(e, message);

            throw RpcExceptionHelper.Internal(message);
        }
    }

    public override async Task Load(
        LoadRequest request,
        IServerStreamWriter<LoadResponse> responseStream,
        ServerCallContext context)
    {
        if (request.AssociationRuleSetId <= 0)
        {
            throw RpcExceptionHelper.InvalidArgument("Association rule set id must be positive.");
        }

        try
        {
            var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

            await using var connection = new NpgsqlConnection(connectionString);

            await connection.OpenAsync(context.CancellationToken);

            await using var transaction = await connection.BeginTransactionAsync(
                context.CancellationToken);

            var command = new CommandDefinition(
                "SELECT pg_advisory_xact_lock_shared(@Id);",
                transaction: transaction,
                parameters: new { Id = request.AssociationRuleSetId },
                cancellationToken: context.CancellationToken);
            await connection.ExecuteScalarAsync(command);

            command = new CommandDefinition(
                """
                SELECT *
                FROM association_rule_sets
                WHERE id = @Id
                    AND is_saving_complete = TRUE
                    AND is_marked_to_delete = FALSE;
                """,
                transaction: transaction,
                parameters: new { Id = request.AssociationRuleSetId },
                cancellationToken: context.CancellationToken);
            var associationRuleSet = await connection.QueryFirstOrDefaultAsync<AssociationRuleSetModel>(command);

            if (associationRuleSet == null)
            {
                await transaction.CommitAsync(context.CancellationToken);
                throw RpcExceptionHelper.NotFound($"Association rule set not found by id {request.AssociationRuleSetId}.");
            }

            await responseStream.WriteAsync(
                new()
                {
                    Part = new()
                    {
                        Set = new()
                        {
                            Id = associationRuleSet.Id,
                            Name = associationRuleSet.Name,
                            Description = associationRuleSet.Description,
                            TransactionsCount = associationRuleSet.TransactionsCount,
                            CreatedAt = associationRuleSet.CreatedAt.ToUniversalTime().ToTimestamp(),
                        }
                    }
                },
                cancellationToken: context.CancellationToken);

            var itemChunks = connection
                .QueryUnbufferedAsync<ItemChunkModel>(
                    "SELECT * FROM item_chunks WHERE association_rule_set_id = @Id",
                    new { Id = request.AssociationRuleSetId },
                    transaction: transaction,
                    commandTimeout: 1800)
                .WithCancellation(context.CancellationToken);

            await foreach (var itemChunk in itemChunks)
            {
                var data = itemChunk.Data as byte[];

                await responseStream.WriteAsync(
                    new() { Part = new() { ItemChunk = ItemChunkGrpc.Parser.ParseFrom(data) } },
                    cancellationToken: context.CancellationToken);
            }

            var associationRuleChunks = connection
                .QueryUnbufferedAsync<AssociationRuleChunkModel>(
                    "SELECT * FROM association_rule_chunks WHERE association_rule_set_id = @Id",
                    new { Id = request.AssociationRuleSetId },
                    transaction: transaction,
                    commandTimeout: 1800)
                .WithCancellation(context.CancellationToken);

            await foreach (var associationRuleChunk in associationRuleChunks)
            {
                var data = associationRuleChunk.Data as byte[];

                await responseStream.WriteAsync(
                    new() { Part = new() { AssociationRuleChunk = AssociationRuleChunkGrpc.Parser.ParseFrom(data) } },
                    cancellationToken: context.CancellationToken);
            }

            await transaction.CommitAsync(context.CancellationToken);
        }
        catch(Exception e)
        {
            const string message = "Unexpected error occurred while loading association rule set.";

            logger.LogError(e, message);

            throw RpcExceptionHelper.Internal(message);
        }
    }
}