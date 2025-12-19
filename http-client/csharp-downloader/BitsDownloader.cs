using System;
using System.Runtime.InteropServices;
using System.Threading;

[ComImport, Guid("4991D34B-80A1-4291-83B6-3328366B9097")]
class BackgroundCopyManager { }

[ComImport, Guid("5CE34C0D-0DC9-4C1F-897C-DAA1B78CEE7C"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
interface IBackgroundCopyManager
{
    void CreateJob([MarshalAs(UnmanagedType.LPWStr)] string displayName, int type, out Guid jobId, out IBackgroundCopyJob job);
}

[ComImport, Guid("37668D37-507E-4160-9316-26306D150B12"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
interface IBackgroundCopyJob
{
    void AddFileSet(uint cFileCount, IntPtr pFileSet);
    void AddFile([MarshalAs(UnmanagedType.LPWStr)] string RemoteUrl, [MarshalAs(UnmanagedType.LPWStr)] string LocalName);
    void EnumFiles(out IntPtr pEnum);
    void Suspend();
    void Resume();
    void Cancel();
    void Complete();
    void GetId(out Guid pVal);
    void GetType(out int pVal);
    void GetProgress(out BG_JOB_PROGRESS pVal);
    void GetTimes(out long creationTime, out long modificationTime, out long transferCompletionTime);
    void GetState(out int pVal);
}

[StructLayout(LayoutKind.Sequential)]
struct BG_JOB_PROGRESS
{
    public ulong BytesTotal;
    public ulong BytesTransferred;
    public uint FilesTotal;
    public uint FilesTransferred;
}

class BitsDownloader
{
    const int BG_JOB_STATE_TRANSFERRED = 6;
    const int BG_JOB_STATE_ERROR = 4;
    const int BG_JOB_STATE_ACKNOWLEDGED = 7;

    static void Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: BitsDownloader.exe <url> <destination>");
            return;
        }

        string url = args[0];
        string destination = System.IO.Path.GetFullPath(args[1]);

        try
        {
            IBackgroundCopyManager manager = (IBackgroundCopyManager)new BackgroundCopyManager();
            IBackgroundCopyJob job;
            Guid jobId;

            manager.CreateJob("Download", 0, out jobId, out job);
            job.AddFile(url, destination);
            job.Resume();

            int state;
            while (true)
            {
                job.GetState(out state);

                if (state == BG_JOB_STATE_TRANSFERRED)
                {
                    job.Complete();
                    break;
                }
                else if (state == BG_JOB_STATE_ERROR)
                {
                    job.Cancel();
                    Console.WriteLine("Transfer failed");
                    return;
                }
                Thread.Sleep(100);
            }

            if (System.IO.File.Exists(destination))
            {
                var info = new System.IO.FileInfo(destination);
                Console.WriteLine("OK " + info.Length + " bytes");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}
