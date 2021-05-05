# FTP Sync Service #

Simple Windows service to sync new files from a targeted directory to an FTP server.

## Build and publish

Run `build` command in the repository root folder, to build the app.

> dotnet build FtpSyncService.sln --configuration Release

Run `publish` command

> dotnet publish -o publish
- Creates `publish` folder in the repository root folder.


## Run application

It is possible to run the app executable or dll via `dotnet` command

> publish\FtpSyncService.exe {{arguments}} {{options}}

> dotnet publish\FtpSyncService.dll {{arguments}} {{options}}

Note: Run empty command to get the standard help

### Arguments

1. Local Path - Local full folder path to watch.
2. Remote path - Remote relative folder path to upload.

### Options

General:
- FilterPattern (`-f, --filter`) - Extension filter pattern [default: \*.*]
- Clean (`-c, --clean`) - Delete files after upload [default: true]
- SyncDelayMiliseconds (`-d, --delay`) - Sync delay (in ms) after file created event [default: 1000]

Ftp:
- Host (`-h, --host`) - Ftp server host address
- Port (`-t, --port`) - Ftp server port [default: 21]
- User (`-u, --user`) - Ftp server user name
- Password (`-p, --password`) -  Ftp server password

### Example

Parametrized:
> FtpSyncService.exe \{local-path\} \{remote-path\} --filter \{filter-expression\} --host \{host-address\} --user \{user\} --password \{password\}

Real:
> FtpSyncService.exe %USERPROFILE%\Downloads DownloadsBackup --filter \*.* --host 127.0.0.1 --user user --password 123456

Note: Filter supports wildcards, e.g. `--filter *.txt` to sync all `.txt` files.

## Create Windows service

Use Service Controller (`sc.exe`) tool to manage Windows Services

Run `create` command to add application as Windows Service

> sc.exe create "Ftp Sync Service" binPath= "\{publish-folder\}\FtpSyncService.exe \{local-path\} \{remote-path\} --filter \{filter-expression\} --host \{host-address\} --user \{user\} --password \{password\}" start= delayed-auto

Note: Once created, service may be manually started or operating system need to be restarted

Run `delete` command to remove the Windows Service, after it has been stopped

> sc.exe delete "Ftp Sync Service"

## Logging

Use Event Viewer tool to get the logs from the `Ftp Sync Service` service.

- `Source: Ftp Sync Service` - use `Source` to filter logs from this service only
