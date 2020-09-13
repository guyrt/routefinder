namespace CosmosDBLayer
{

    using System;
    using Microsoft.Extensions.Configuration;

    public static class SettingsManager
    {

        public static CosmosCredentials GetCredentials()
        {
            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            var configuration = configurationBuilder.AddJsonFile("appsettings.json").Build();

            string endpoint = configuration["EndPointUrl"];
            string authKey = configuration["AuthorizationKey"];

            return new CosmosCredentials
            {
                AuthKey = authKey,
                EndPoint = endpoint,
                LocalFilePattern = configuration["LocalFilePattern"],
                LocalBz2FilePattern = configuration["LocalBz2FilePattern"],
                RemoteFile = configuration["RemoteFile"],
                RemoteMd5File = configuration["RemoteMd5File"]
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
        }
    }
}
