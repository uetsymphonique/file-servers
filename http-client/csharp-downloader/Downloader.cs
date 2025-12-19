using System;
using System.IO;
using System.Net;

class Downloader
{
    static void Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: Downloader.exe <url> <destination>");
            return;
        }

        string url = args[0];
        string destination = args[1];

        try
        {
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(url, destination);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}

