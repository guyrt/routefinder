using Newtonsoft.Json;
using RouteCleaner;
using RouteCleaner.Model;
using System.Collections.Generic;
using System.IO;

namespace OpenStreetMapEtl.Utils
{
    public class DetailedDebugOutputter
    {
        private string _path;

        public DetailedDebugOutputter(string path)
        {
            _path = path;
        }

        public void OutputWays(IEnumerable<Way> ways, string filename)
        {
            var fullPath = Path.Combine(_path, filename);
            var converter = new GeoJsonConverter();
            var polygonOut = converter.Convert(ways);
            var serialized = JsonConvert.SerializeObject(polygonOut);
            File.WriteAllLines(fullPath, new[] { serialized });
        }
    }
}
