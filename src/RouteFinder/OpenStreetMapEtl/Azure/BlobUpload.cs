using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using OpenStreetMapEtl.Utils;
using System;
using System.IO;
using System.Text;

namespace OpenStreetMapEtl.Azure
{
    internal class BlobUpload
    {
        private readonly string _path = "jsonraw";
        private readonly string _suffix = "json";

        public string Upload(string geometry, BoundingBox box)
        {
            var path = BoundingBoxFilenameConverter.CreateFileName(box, _suffix);

            // todo: move this.
            string storageConnectionString = Environment.GetEnvironmentVariable("");

            if (CloudStorageAccount.TryParse(storageConnectionString, out var storageAccount))
            {
                try
                {
                    CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();
                    CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference(_path);
                    CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(path);
                    cloudBlockBlob.UploadFromStream(new MemoryStream(Encoding.ASCII.GetBytes(geometry)));
                }
                catch (StorageException ex)
                {
                    Console.WriteLine(ex);
                }
            }

            return path;
        }

    }
}
