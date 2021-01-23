using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DailyDesktopBackground.Helper;
using DailyDesktopBackground.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DailyDesktopBackground.Services
{
    public class TimedHostedService : IHostedService, IDisposable
    {
        private int _executionCount = 0;
        private readonly ILogger<TimedHostedService> _logger;
        private readonly IConfiguration _config;
        private Timer _timer;

        public TimedHostedService(ILogger<TimedHostedService> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service running.");

            var waitTime = _config["waitTime"];

            _timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromMinutes(string.IsNullOrEmpty(waitTime) ? 60 : Convert.ToInt32(waitTime)));

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            var count = Interlocked.Increment(ref _executionCount);

            _logger.LogInformation(
                "Timed Hosted Service is working. Count: {Count}", count);

            try
            {
                var unsplashApi = new UnsplashApiClient(_config);
                var wallpaperCom = (IDesktopWallpaper) (new DesktopWallpaperClass());
                for (uint i = 0; i < wallpaperCom.GetMonitorDevicePathCount(); i++)
                {
                    var monitorId = wallpaperCom.GetMonitorDevicePathAt(i);

                    if(string.IsNullOrEmpty(monitorId)) continue;

                    var photo = unsplashApi.GetRandomDesktopBackground().GetAwaiter().GetResult();
                    var outputPath = _config["outputPath"];
                    var imagePath = string.IsNullOrEmpty(outputPath)
                        ? Path.Combine(Path.GetTempPath(), "wallpaper.jpg")
                        : Path.Combine(outputPath, $"{DateTime.Now:yyyyMMddHHmm}_{photo.Id}.jpg");
                    WallpaperHelper.SetWallpaper(wallpaperCom, monitorId, imagePath, new Uri(photo.Urls.Full));
                    unsplashApi.CallDownloadTrackingEndpoint(photo);
                }

            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
