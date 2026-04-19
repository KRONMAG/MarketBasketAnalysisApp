using Grpc.Core;

namespace MarketBasketAnalysisStorage.Api.Extensions;

public static class RpcExceptionHelper
{
    public static RpcException InvalidArgument(string detail) =>
        GetRpcException(StatusCode.InvalidArgument, detail);

    public static RpcException Internal(string detail) =>
        GetRpcException(StatusCode.Internal, detail);

    public static RpcException NotFound(string detail) =>
        GetRpcException(StatusCode.NotFound, detail);

    public static RpcException FailedPrecondition(string detail) =>
        GetRpcException(StatusCode.FailedPrecondition, detail);

    private static RpcException GetRpcException(StatusCode statusCode, string detail) =>
        new RpcException(new Status(statusCode, detail));
}