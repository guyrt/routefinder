using System;
using Newtonsoft.Json;
using RouteFinderDataModel;

namespace RouteFinderCmd
{
    /// <summary>
    /// Store output files from a processed set of OSM data.
    /// </summary>
    public class WritePreppedData
    {
        public void OutputGeometry(Geometry geometry)
        {
            foreach (var node in geometry.Nodes)
            {
                var nodeStr = JsonConvert.SerializeObject(node);
                Console.WriteLine(nodeStr);
            }
        }
    }
}
