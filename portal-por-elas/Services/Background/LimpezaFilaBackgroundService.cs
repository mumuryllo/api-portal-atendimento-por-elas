using portal_por_elas.Interfaces.Services;

namespace portal_por_elas.Services.Background
{
    public class LimpezaFilaBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<LimpezaFilaBackgroundService> _logger;
        private static readonly TimeSpan IntervaloVerificacao = TimeSpan.FromMinutes(10);

        public LimpezaFilaBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<LimpezaFilaBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var service = scope.ServiceProvider.GetRequiredService<IGestaoFilaService>();
                    var removidos = await service.LimparAtendimentosFinalizadosExpiradosAsync();

                    if (removidos > 0)
                        _logger.LogInformation("Limpeza automática: {Qtd} atendimento(s) finalizado(s) expirado(s) removido(s).", removidos);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao executar limpeza automática de atendimentos finalizados.");
                }

                await Task.Delay(IntervaloVerificacao, stoppingToken);
            }
        }
    }
}
