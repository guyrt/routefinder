using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public async Task<bool> RetrieveBlobAsync(string remoteFileName, string localFileName)
        {
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
