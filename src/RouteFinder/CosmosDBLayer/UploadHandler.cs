namespace CosmosDBLayer
{
    using Microsoft.Azure.Cosmos;
    using RouteFinderDataModel;
    using System.Collections.Generic;

    public class UploadHandler
    {
        private readonly string _endpoint;

        private readonly string _authKey;

        public UploadHandler(string endPoint, string authKey)
        {
            _endpoint = endPoint;
            _authKey = authKey;
        }

        public async void Upload(IEnumerable<Way> ways)
        {
            using (var client = new CosmosClient(_endpoint, _authKey))
            {
                var uploader = new Uploader(client);
                await uploader.Initialize();

                foreach (var way in ways)
                {
                    await uploader.Upload(way);
                }
            }
        }
    }
}
