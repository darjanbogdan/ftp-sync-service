using CommandLine;

namespace FtpSyncService.Configuration
{
    public class FtpUploaderOptions
    {
        [Value(index: 0, Required = true, HelpText = "Local folder path to watch.")]
        public string LocalPath { get; init; }

        [Value(index: 1, Required = true, HelpText = "Remote folder path to upload.")]
        public string RemotePath { get; init; }

        [Option(shortName: 'f', longName: "filter", Required = false, HelpText = "Extension filter pattern", Default = "*.*")]
        public string FilterPattern { get; init; }

        [Option(shortName: 'c', longName: "clean", Required = false, HelpText = "Delete files after upload", Default = true)]
        public bool Clean { get; init; }

        [Option(shortName: 'd', longName: "delay", Required = false, HelpText = "Sync delay (in ms) after file created event", Default = 1000)]
        public int SyncDelayMiliseconds { get; init; }

        [Option(shortName: 'h', longName: "host", Required = false, HelpText = "Ftp server host address")]
        public string Host { get; set; }

        [Option(shortName: 't', longName: "port", Required = false, HelpText = "Ftp server port", Default = 21)]
        public int Port { get; init; }

        [Option(shortName: 'u', longName: "user", Required = false, HelpText = "Ftp server user name")]
        public string User { get; init; }

        [Option(shortName: 'p', longName: "password", Required = false, HelpText = "Ftp server password")]
        public string Password { get; set; }
    }
}
