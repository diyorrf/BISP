using back.Data.Repos.Interfaces;

namespace back.Services.Regulatory;

public class RegulatoryMonitorBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RegulatoryMonitorBackgroundService> _logger;
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(5);

    public RegulatoryMonitorBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<RegulatoryMonitorBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Regulatory monitor background service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessUnmatchedUpdatesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in regulatory monitor background service");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task ProcessUnmatchedUpdatesAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var updateRepo = scope.ServiceProvider.GetRequiredService<IRegulatoryUpdateRepository>();
        var matchingService = scope.ServiceProvider.GetRequiredService<IRegulatoryMatchingService>();

        var unprocessed = await updateRepo.GetUnprocessedAsync(ct);

        foreach (var update in unprocessed)
        {
            _logger.LogInformation("Processing regulatory update {UpdateId}: {Title}", update.Id, update.Title);

            var alertsCreated = await matchingService.MatchAndCreateAlertsAsync(update, ct);

            update.IsProcessed = true;
            await updateRepo.UpdateAsync(update, ct);

            _logger.LogInformation("Regulatory update {UpdateId} processed, {Alerts} alerts created", update.Id, alertsCreated);
        }
    }
}
