using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Text;

class FolderSynchronizer
{
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    private string sourceFolder;
    private string replicaFolder;
    private string logFilePath;
    private int synchronizationInterval;

    public FolderSynchronizer(string sourceFolder, string replicaFolder, string logFilePath, int synchronizationInterval)
    {
        this.sourceFolder = sourceFolder;
        this.replicaFolder = replicaFolder;
        this.logFilePath = logFilePath;
        this.synchronizationInterval = synchronizationInterval;
    }

    public void SynchronizeFolders()
    {
        while (true)
        {
            try
            {
                log.Info("Synchronizing folders...");

                string[] sourceFiles = Directory.GetFiles(sourceFolder, "*", SearchOption.AllDirectories);

                if (!Directory.Exists(replicaFolder))
                {
                    Directory.CreateDirectory(replicaFolder);
                }

                string[] replicaFiles = Directory.GetFiles(replicaFolder, "*", SearchOption.AllDirectories);
                foreach (string replicaFile in replicaFiles)
                {
                    string sourceFile = replicaFile.Replace(replicaFolder, sourceFolder);
                    if (!File.Exists(sourceFile))
                    {
                        File.Delete(replicaFile);
                        log.Info($"Deleted file {replicaFile}");
                    }
                }

                foreach (string sourceFile in sourceFiles)
                {
                    string replicaFile = sourceFile.Replace(sourceFolder, replicaFolder);
                    if (!File.Exists(replicaFile) || GetFileHash(sourceFile) != GetFileHash(replicaFile))
                    {
                        File.Copy(sourceFile, replicaFile, true);
                        log.Info($"Copied file {sourceFile} to {replicaFile}");
                    }
                }

                log.Info("Folders synchronized.");
            }
            catch (Exception ex)
            {
                log.Error("Error synchronizing folders: " + ex.Message);
            }

            Thread.Sleep(synchronizationInterval * 1000);
        }
    }

    private string GetFileHash(string filePath)
    {
        using (var md5 = MD5.Create())
        {
            using (var stream = File.OpenRead(filePath))
            {
                var hash = md5.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}
class Program
{
    static void Main(string[] args)
    {
        if (args.Length != 4)
        {
            Console.WriteLine("Usage: FolderSynchronizer <sourceFolder> <replicaFolder> <logFilePath> <synchronizationInterval>");
            return;
        }

        string sourceFolder = args[0];
        string replicaFolder = args[1];
        string logFilePath = args[2];
        int synchronizationInterval = int.Parse(args[3]);

        FolderSynchronizer synchronizer = new FolderSynchronizer(sourceFolder, replicaFolder, logFilePath, synchronizationInterval);
        synchronizer.SynchronizeFolders();
    }
}

