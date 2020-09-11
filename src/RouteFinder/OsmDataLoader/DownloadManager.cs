namespace OsmDataLoader
{
    using ICSharpCode.SharpZipLib.BZip2;
    using System;
    using System.IO;
    using System.Net;
    using System.Security.Cryptography;
    using System.Text;

    public class DownloadManager
    {
        private readonly DownloadConfig _config;

        public DownloadManager(DownloadConfig config)
        {
            _config = config;
        }

        /// <summary>
        /// Perform download if needed.
        /// </summary>
        /// <returns></returns>
        public string DownloadAndUnzip()
        {
            var remoteHash = GetRemoteHash(_config.RemoveMd5File);
            var localFile = string.Format(_config.LocalFilePattern, remoteHash);
            var localTmpFile = string.Format(_config.LocalFilePattern, "tmp");
            var localBz2File = string.Format(_config.LocalBz2FilePattern, remoteHash);

            if (File.Exists(localFile))
            {
                Console.WriteLine("Local file has matching hash. No download needed.");
                return localFile;
            }

            Console.WriteLine($"Downloading from {_config.RemoteFile}");
            
            using (var client = new WebClient())
            {
                client.DownloadFile(_config.RemoteFile, localBz2File);
            }

            Console.WriteLine($"Done downloading. Unzipping to tmp file");
            UnZip(localBz2File, localTmpFile);
            Console.WriteLine($"Moving file to final file {localFile}");
            File.Move(localTmpFile, localFile);
            Console.WriteLine($"File download complete.");
            return localFile;
        }

        private void UnZip(string localBz2File, string localFile)
        {
            using (FileStream fileToDecompressAsStream = File.OpenRead(localBz2File))
            {
                string decompressedFileName = localFile;
                using (FileStream decompressedStream = File.Create(decompressedFileName))
                {
                    try
                    {
                        BZip2.Decompress(fileToDecompressAsStream, decompressedStream, true);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        private string GetRemoteHash(string hashUrl)
        {
            using (var wc = new WebClient()) {
                var contents = wc.DownloadString(hashUrl);
                return contents.Split(' ')[0].ToLower();
            }
        }

        private string GetLocalFileHash(string filePath)
        {
            if (File.Exists(filePath))
            {
                string text = File.ReadAllText(@"C:\Users\Public\TestFolder\WriteText.txt");
                return text.Split(' ')[0].ToLower();
            }
            return string.Empty;
        }
    }
}
