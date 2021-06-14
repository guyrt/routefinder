using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;

namespace AzureBlobHandler
{
    /// <summary>
    /// Upload handler for raw OSM data
    /// </summary>
    public class RawDataUploader
    {
        private string connectionString;

        private string containerName;

        private BlobContainerClient containerClient;

        private bool initialized;

        public RawDataUploader(string connectionString, string container)
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

        public async Task<bool> WriteBlobAsync(string fileName, byte[] content)
        {
            if (!this.initialized)
            {
                this.Initialize();
            }

            BlobClient blobClient = containerClient.GetBlobClient(fileName);
            using (var memStream = new MemoryStream(content, writable: false))
            {
                var response = await blobClient.UploadAsync(memStream, overwrite: true);
            }
            return true;
        }
    }
}
