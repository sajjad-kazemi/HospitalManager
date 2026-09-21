using Grpc.Core;
using Grpc.Core.Interceptors;
using System.Security.Cryptography;
using System.Text;

namespace IdentityService.Api.Grpc
{
    public class ServiceKeyInterceptor(IConfiguration configuration): Interceptor
    {
        public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
            TRequest request,
            ServerCallContext context,
            UnaryServerMethod<TRequest, TResponse> continuation)
        {
            var expected = configuration["InternalGrpc:ServiceKey"]
            ?? throw new InvalidOperationException(
                "Missing configuration: InternalGrpc:ServiceKey");

            var supplied = context.RequestHeaders.GetValue("x-service-key");

            var expectedHash = SHA256.HashData(Encoding.UTF8.GetBytes(expected));
            var suppliedHash = SHA256.HashData(
                Encoding.UTF8.GetBytes(supplied ?? string.Empty));


            if (string.IsNullOrEmpty(supplied) ||
                !CryptographicOperations.FixedTimeEquals(
                    expectedHash, suppliedHash))
            {
                throw new RpcException(new Status(
                    StatusCode.PermissionDenied,
                    "Service authentication failed."));
            }

            return continuation(request, context);
        }
    }
}
