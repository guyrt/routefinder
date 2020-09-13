using System.Collections.Generic;

namespace RouteFinderDataModel.Thin
{
    public class ThinNode
    {
        public string Id { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public Dictionary<string, string> Tags { get; set; }

        public Node ToThick()
        {
            return new Node(Id, Latitude, Longitude, Tags);
        }
    }
}
