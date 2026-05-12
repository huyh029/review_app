using reviewApi.Service.Auth;

namespace reviewApi.Service.Repositories.Auth
{
    /// <summary>
    /// Background service: tự động đồng bộ Keycloak → DB lúc 00:00 hàng ngày.
    /// </summary>
    public class KeycloakSyncBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<KeycloakSyncBackgroundService> _logger;

        public KeycloakSyncBackgroundService(IServiceScopeFactory scopeFactory,
            ILogger<KeycloakSyncBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger       = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("[SyncJob] Background sync service đã khởi động.");

            while (!stoppingToken.IsCancellationRequested)
            {
                var delay = TimeUntilMidnight();
                _logger.LogInformation("[SyncJob] Sync tiếp theo lúc 00:00 — còn {Hours}h {Minutes}m.",
                    (int)delay.TotalHours, delay.Minutes);

                try
                {
                    await Task.Delay(delay, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }

                if (stoppingToken.IsCancellationRequested) break;

                await RunSyncAsync();
            }

            _logger.LogInformation("[SyncJob] Background sync service đã dừng.");
        }

        private async Task RunSyncAsync()
        {
            _logger.LogInformation("[SyncJob] Bắt đầu sync lúc 00:00...");
            try
            {
                // BackgroundService dùng singleton scope → phải tạo scope mới để dùng scoped services
                using var scope = _scopeFactory.CreateScope();
                var syncService = scope.ServiceProvider.GetRequiredService<IKeycloakSyncService>();
                await syncService.SyncAllAsync();
                _logger.LogInformation("[SyncJob] Sync 00:00 hoàn thành.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SyncJob] Sync 00:00 thất bại: {Message}", ex.Message);
                // Không throw — để service tiếp tục chạy ngày hôm sau
            }
        }

        /// <summary>Tính thời gian còn lại đến 00:00 ngày hôm sau.</summary>
        private static TimeSpan TimeUntilMidnight()
        {
            var now       = DateTime.Now;
            var midnight  = now.Date.AddDays(1); // 00:00 ngày mai
            return midnight - now;
        }
    }
}
