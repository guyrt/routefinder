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

        public async Task<bool> UploadFileAsync(string remoteFileName, string localFileName)
        {
            if (!this.initialized)
            {
                this.Initialize();
            }

            BlobClient blobClient = containerClient.GetBlobClient(remoteFileName);
            await blobClient.UploadAsync(localFileName, true);
            return true;
        }

        public async Task<bool> WriteBlobAsync(string remoteFileName, byte[] localContents)
        {
            if (!this.initialized)
            {
                this.Initialize();
            }

            BlobClient blobClient = containerClient.GetBlobClient(remoteFileName);
            using (var stream = new MemoryStream(localContents, writable: false))
            {
                await blobClient.UploadAsync(stream, true);
            };
            return true;
        }
    }
}
