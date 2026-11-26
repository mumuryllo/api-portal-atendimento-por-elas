using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace portal_por_elas.Extensions
{
    public static class RateLimitingExtension
    {
        public const string PoliticaGeral = "geral";
        public const string PoliticaAuthRestrito = "auth-restrito";

        public static IServiceCollection AddCustomRateLimiting(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.OnRejected = async (context, cancellationToken) =>
                {
                    context.HttpContext.Response.ContentType = "application/json";
                    await context.HttpContext.Response.WriteAsJsonAsync(new
                    {
                        statusCode = 429,
                        error = "Muitas requisições em pouco tempo. Aguarde alguns instantes e tente novamente."
                    }, cancellationToken);
                };

                // Política Geral (Painéis e listagens com throttling via fila)
                options.AddPolicy(PoliticaGeral, httpContext =>
                    RateLimitPartition.GetSlidingWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
                        factory: _ => new SlidingWindowRateLimiterOptions
                        {
                            PermitLimit = 100,
                            Window = TimeSpan.FromMinutes(1),
                            SegmentsPerWindow = 4,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 5 // Throttling: enfileira até 5 requisições excedentes antes de rejeitar
                        }));

                // Política Estrita (Login e Recuperação de Senha)
                options.AddPolicy(PoliticaAuthRestrito, httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 5,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 0 // Rejeita imediatamente com 429 na 6ª tentativa
                        }));
            });

            return services;
        }
    }
}