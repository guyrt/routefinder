using System;
using System.IO;
using System.Threading.Tasks;
using AzureBlobHandler;
using Google.Protobuf;
using Newtonsoft.Json;
using OsmETL;
using RouteFinderDataModel.Proto;

namespace DataInspector
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            var config = SettingsManager.GetCredentials();
            var rawDataDownloader = new DataDownloadWrapper(config.AzureRawXmlDownloadConnectionString, config.AzureBlobProcessedNodesContainer);

            var key = "84VVRP00+";

            var localNodeFile = $"/tmp/{key}";
            var localWayFile = $"/tmp/ways_{key}";
            var remoteNodeFileName = $"nodes/{key[..2]}/{key}";
            var remoteWayFileName = $"ways/{key[..2]}/{key}";
            await DownloadAndDecodeAsync(rawDataDownloader, remoteNodeFileName, localNodeFile, (FileStream s) => FullNodeSet.Parser.ParseFrom(s));
            await DownloadAndDecodeAsync(rawDataDownloader, remoteWayFileName, localWayFile, (FileStream s) => FullWaySet.Parser.ParseFrom(s));
        }

        private static async Task DownloadAndDecodeAsync<T>(DataDownloadWrapper rawDataDownloader, string remoteFile, string localFile, Func<FileStream, T> parser)
            where T : IMessage<T>
        {
            await rawDataDownloader.RetrieveBlobAsync(remoteFile, localFile);

            var jsonFile = $"{localFile}.json";
            T area;
            using var input = File.OpenRead(localFile);
            area = parser(input);
            var areaStr = JsonConvert.SerializeObject(area, Formatting.Indented);
            File.WriteAllText(jsonFile, areaStr);
        }
    }
}
