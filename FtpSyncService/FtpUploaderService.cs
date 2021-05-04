using FluentFTP;
using FtpSyncService.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FtpSyncService
{
    public class FtpUploaderService : BackgroundService
    {
        private readonly FtpUploaderOptions _options;
        private readonly ILogger<FtpUploaderService> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public FtpUploaderService(FtpUploaderOptions options, ILogger<FtpUploaderService> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _options = options;
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Ftp uploader service started.");
            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Ftp uploader service stopped.");
            return base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using FileSystemWatcher watcher = new FileSystemWatcher(_options.LocalPath, _options.FilterPattern) { EnableRaisingEvents = true };
            watcher.Created += FileCreated;
            
            await ExecutionStopped(stoppingToken);

            async void FileCreated(object sender, FileSystemEventArgs e)
            {
                _logger.LogInformation($"File {e.FullPath} created.");
                await Task.Delay(_options.SyncDelayMiliseconds, stoppingToken);

                try
                {
                    await BackupFileAsync(e.FullPath, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"File {e.FullPath} backup failed.");
                }
            }

            async Task ExecutionStopped(CancellationToken stoppingToken)
            {
                var executeCompletionSource = new TaskCompletionSource<bool>();
                stoppingToken.Register(s => ((TaskCompletionSource<bool>)s).SetResult(true), executeCompletionSource);
                await executeCompletionSource.Task;
            }
        }

        private async Task BackupFileAsync(string localFilePath, CancellationToken stoppingToken)
        {
            using IServiceScope scope = _serviceScopeFactory.CreateScope();
            var ftpClient = scope.ServiceProvider.GetRequiredService<FtpClient>();

            await ftpClient.ConnectAsync(stoppingToken);

            FileInfo fileInfo = new(localFilePath);
            string remoteFilePath = Path.Combine(_options.RemotePath, fileInfo.Name);

            FtpStatus status = await ftpClient.UploadFileAsync(localFilePath, remoteFilePath, token: stoppingToken);

            Action afterUploadAction = status switch
            {
                FtpStatus.Failed => () => _logger.LogInformation($"The file {fileInfo.Name} upload failed with an error transfering, or the source file did not exist"),
                FtpStatus.Success => () =>
                {
                    _logger.LogInformation($"File {fileInfo.Name} uploaded.");

                    DeleteLocalFile(fileInfo);
                },
                _ => () => { }
            };

            afterUploadAction();
        }

        private void DeleteLocalFile(FileInfo fileInfo)
        {
            if (_options.Clean is false) return;
            
            try
            {
                File.Delete(fileInfo.FullName);
                _logger.LogInformation($"File {fileInfo.Name} deleted.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"File {fileInfo.Name} delete failed.");
            }
        }
    }
}
