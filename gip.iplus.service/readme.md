## gip.iplus.service

This project is the .NET (Generic Host) service port of the old .NET Framework Windows Service.

Current hosting model:

- Windows: `UseWindowsService()`
- Linux: `UseSystemd()`
- Startup logic runs in `IPlusBackgroundService.StartAsync()`

If login to iPlus fails during startup, the service now fails fast (throws) so SCM/systemd sees the start as failed.

## Command line arguments

Supported runtime arguments are passed directly to `CommandLineHelper`:

- `/U...` user
- `/P...` password
- `/WCFOff`
- `/Simulation`
- `/Debug` (waits 60 seconds before login to allow debugger attach)

If no args are provided, defaults are `/U00 /P00`.

## Build and publish

Windows x64 publish example:

```bash
dotnet publish ./gip.iplus.service/gip.iplus.service.csproj -c Release -r win-x64 --self-contained false -o ./publish/win-x64
```

Linux x64 publish example:

```bash
dotnet publish ./gip.iplus.service/gip.iplus.service.csproj -c Release -r linux-x64 --self-contained false -o ./publish/linux-x64
```

## Install on Windows

Run an elevated terminal (`cmd` or PowerShell):

```cmd
sc.exe create IPlusService binPath= "C:\Apps\iPlus\gip.iplus.service.exe /U00 /P00" start= auto
sc.exe description IPlusService "iPlus backend service"
sc.exe start IPlusService
```

Check status:

```cmd
sc.exe query IPlusService
```

Update args or executable path:

```cmd
sc.exe config IPlusService binPath= "C:\Apps\iPlus\gip.iplus.service.exe /U00 /P00 /WCFOff"
```

Uninstall:

```cmd
sc.exe stop IPlusService
sc.exe delete IPlusService
```

## Install on Linux (systemd)

1. Copy published files, for example to `/opt/iplus-service`.
2. Create `/etc/systemd/system/iplus.service`:

```ini
[Unit]
Description=iPlus Service
After=network.target

[Service]
Type=notify
WorkingDirectory=/opt/iplus-service
ExecStart=/opt/iplus-service/gip.iplus.service /U00 /P00
Restart=always
RestartSec=5

[Install]
WantedBy=multi-user.target
```

3. Enable and start:

```bash
sudo systemctl daemon-reload
sudo systemctl enable iplus.service
sudo systemctl start iplus.service
```

Check status and logs:

```bash
sudo systemctl status iplus.service
sudo journalctl -u iplus.service -f
```

Uninstall:

```bash
sudo systemctl stop iplus.service
sudo systemctl disable iplus.service
sudo rm /etc/systemd/system/iplus.service
sudo systemctl daemon-reload
```

## Notes about legacy files

`Service1*.cs` and `ProjectInstaller*.cs` are legacy .NET Framework service artifacts and are currently commented out. The active runtime path for .NET is `Program.cs` + `IPlusBackgroundService.cs`.
