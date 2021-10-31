namespace OsmETL
{
    using Microsoft.Extensions.Configuration;

    public static class SettingsManager
    {

        public static CosmosCredentials GetCredentials()
        {
            var configurationBuilder = new ConfigurationBuilder();
            var configuration = configurationBuilder
                                    .AddJsonFile("appsettings.json")
                                    .AddJsonFile("credentials.json")   // store credentials. See credentials.sample.json for structure
                                    .Build();

            string endpoint = configuration["EndPointUrl"];
            string authKey = configuration["AuthorizationKey"];

            return new CosmosCredentials
            {
                AuthKey = authKey,
                EndPoint = endpoint,
                LocalFilePattern = configuration["LocalFilePattern"],
                LocalBz2FilePattern = configuration["LocalBz2FilePattern"],
                RemoteFile = configuration["RemoteFile"],
                RemoteMd5File = configuration["RemoteMd5File"],
                CosmosDatabase = configuration["CosmosDatabase"],
                CosmosContainer = configuration["CosmosContainer"],
                AzureBlobProcessedNodesContainer = configuration["AzureBlobProcessedNodesContainer"],
                AzureRawXmlDownloadConnectionString = configuration["AzureRawXmlDownloadConnectionString"],
                AzureRawXmlDownloadContainer = configuration["AzureRawXmlDownloadContainer"],
            };
        }

        public class CosmosCredentials
        {
            public string AuthKey { get; set; }

            public string EndPoint { get; set; }

            public string LocalFilePattern { get; set; }

            public string LocalBz2FilePattern { get; set; }

            public string RemoteFile { get; set; }

            public string RemoteMd5File { get; set; }

            public string CosmosDatabase { get; set; }

            public string CosmosContainer { get; set; }
            
            public string AzureRawXmlDownloadConnectionString { get; set; }

            public string AzureRawXmlDownloadContainer { get; set; }

            /// <summary>
            /// Location for processed nodes - these have run through ProcessRunnableWays.
            /// </summary>
            public string AzureBlobProcessedNodesContainer { get; set; }

        }
    }
}
