using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

// 1. CORS centralizado no Gateway para os micro-frontends (Admin e Client)
builder.Services.AddCors(options =>
{
    options.AddPolicy("GatewayCorsPolicy", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:3000",
                "http://localhost:3001",
                "http://localhost:3002"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// 2. Rate Limiting de Borda (protege a API interna contra sobrecarga e força bruta)
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddFixedWindowLimiter("gateway-auth-limit", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 10;
        opt.QueueLimit = 0;
    });

    options.AddFixedWindowLimiter("gateway-default-limit", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 100;
        opt.QueueLimit = 5;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
});

// 3. Registra o YARP Reverse Proxy lendo do appsettings.json e injetando X-Correlation-Id
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(transformBuilderContext =>
    {
        transformBuilderContext.AddRequestTransform(transformContext =>
        {
            var correlationId = transformContext.HttpContext.Request.Headers["X-Correlation-Id"].FirstOrDefault()
                                ?? Guid.NewGuid().ToString();

            transformContext.ProxyRequest.Headers.Remove("X-Correlation-Id");
            transformContext.ProxyRequest.Headers.Add("X-Correlation-Id", correlationId);
            transformContext.HttpContext.Response.Headers["X-Correlation-Id"] = correlationId;

            return ValueTask.CompletedTask;
        });
    });

var app = builder.Build();

app.UseCors("GatewayCorsPolicy");
app.UseRateLimiter();

// Endpoint rápido de status do próprio Gateway
app.MapGet("/gateway/health", () => Results.Ok(new
{
    status = "Healthy",
    service = "PortalPorElas.Gateway",
    timestampUtc = DateTime.UtcNow
}));

// Encaminha todas as requisições configuradas para os clusters de destino
app.MapReverseProxy();

app.Run();

public partial class Program { }