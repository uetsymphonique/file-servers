@echo off
echo Building WebClient downloader...
csc.exe /out:Downloader.exe /optimize+ Downloader.cs

echo Building BITS downloader...
csc.exe /out:BitsDownloader.exe /optimize+ BitsDownloader.cs

