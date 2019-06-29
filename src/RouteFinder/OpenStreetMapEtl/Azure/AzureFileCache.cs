using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using OpenStreetMapEtl.Utils;
using RouteCleaner;
using RouteCleaner.Model;

namespace OpenStreetMapEtl.Azure
{
    public class AzureFileCache : IFileCache
    {
        private readonly string _connectionString = "DefaultEndpointsProtocol=https;AccountName=routefinderrawdata;AccountKey=Hsm4ANRD7oCAv0Gk/gAgNW6vnoRPGKBdFKAYFHbONdAdANa9woCJAu3dqeK69TEnwa3ULvTqKdXIP/2LShpH0w==;BlobEndpoint=https://routefinderrawdata.blob.core.windows.net/;TableEndpoint=https://routefinderrawdata.table.core.windows.net/;QueueEndpoint=https://routefinderrawdata.queue.core.windows.net/;FileEndpoint=https://routefinderrawdata.file.core.windows.net/";

        private readonly string _containerName = "jsonraw";

        private readonly CloudBlobContainer _container;

        public AzureFileCache()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_connectionString);

            var blobClient = storageAccount.CreateCloudBlobClient();
            _container = blobClient.GetContainerReference(_containerName);
        }

        public Geometry GetBox(BoundingBox box)
        {
            var filename = BoundingBoxFilenameConverter.CreateFileName(box, "json");  // todo
            var blobReference = _container.GetBlobReference(filename);
            string text;
            using (var memoryStream = new MemoryStream())
            {
                blobReference.DownloadToStream(memoryStream);
                text = System.Text.Encoding.UTF8.GetString(memoryStream.ToArray());
            }
            return JsonSerDe.GetGeometry(text);
        }

        /// <summary>
        /// todo - https://docs.microsoft.com/en-us/azure/storage/blobs/storage-quickstart-blobs-dotnet?tabs=windows#list-the-blobs-in-a-container
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CloudBlockBlob> GetFiles()
        {
            var blobList = _container.ListBlobs(useFlatBlobListing: true);
            return blobList.OfType<CloudBlockBlob>();
        }

        public bool RemoveFile(string fileName)
        {
            var blobReference = _container.GetBlobReference(fileName);
            return blobReference.DeleteIfExists();
        }

        public BoundingBox[] ListBoxes()
        {
            var blobList = _container.ListBlobs(useFlatBlobListing: true);
            var listOfFileNames = new List<BoundingBox>();

            foreach (var blob in blobList)
            {
                var blobFileName = blob.Uri.Segments.Last();
                listOfFileNames.Add(BoundingBoxFilenameConverter.ParseFileName(blobFileName));
            }

            return listOfFileNames.ToArray();
        }

    }
}
