using NyayaSetu.Interfaces;

namespace NyayaSetu.Services
{
    public class OverdueEscalationBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OverdueEscalationBackgroundService> _logger;

        public OverdueEscalationBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<OverdueEscalationBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Initial short delay so startup seeding completes first
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var complaintService = scope.ServiceProvider.GetRequiredService<IComplaintService>();
                    var escalated = await complaintService.RunOverdueEscalationCheckAsync();
                    if (escalated > 0)
                    {
                        _logger.LogInformation("Auto-escalated {Count} overdue complaints.", escalated);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while running overdue complaint escalation check.");
                }

                await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
            }
        }
    }
}
