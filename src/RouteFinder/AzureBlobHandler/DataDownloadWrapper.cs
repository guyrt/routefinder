using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;

namespace AzureBlobHandler
{
    /// <summary>
    /// Download handler for raw data
    /// </summary>
    public class DataDownloadWrapper
    {
        private string connectionString;

        private string containerName;

        private BlobContainerClient containerClient;

        private bool initialized;

        public DataDownloadWrapper(string connectionString, string container)
        {
            this.connectionString = connectionString;
            this.containerName = container;
            this.initialized = false;
        }

        public void Initialize()
        {
            var blobServiceClient = new BlobServiceClient(connectionString);
            this.containerClient = blobServiceClient.GetBlobContainerClient(this.containerName);
            this.initialized = true;
        }

        public async Task<bool> RetrieveBlobAsync(string remoteFileName, string localFileName, bool overwrite=false)
        {
            if (!overwrite && File.Exists(localFileName))
            {
                Console.WriteLine($"Skipping downloading {remoteFileName} to {localFileName}.");
                return false;
            }

            if (!this.initialized)
            {
                this.Initialize();
            }

            BlobClient blobClient = containerClient.GetBlobClient(remoteFileName);
            await blobClient.DownloadToAsync(localFileName);

            return true;
        }

    }
}
