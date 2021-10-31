using System.IO;
using System.Threading.Tasks;
using AzureBlobHandler;
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

            var key = "84QVJF00+";
            var localFile = $"/tmp/{key}";
            var remoteFileName = $"nodes/{key.Substring(0, 2)}/{key}";
            await rawDataDownloader.RetrieveBlobAsync(remoteFileName, localFile);

            var jsonFile = $"{localFile}.json";
            FullNodeSet area;
            using (var input = File.OpenRead(localFile))
            {
                area = FullNodeSet.Parser.ParseFrom(input);
                var areaStr = JsonConvert.SerializeObject(area);
                File.WriteAllText(jsonFile, areaStr);
            }

        }
    }
}
