namespace RouteFinderDataModel
{
    using Microsoft.Azure.Cosmos.Spatial;
    using Newtonsoft.Json;
    using RouteFinderDataModel.Tools;
    using System.Collections.Generic;

    public class Way : TaggableIdentifiableElement
    {
        public Way(string id, Node[] nodes, Dictionary<string, string> tags = null, Relation containedIn = null) : base(id, tags)
        {
            Nodes = nodes;
            ContainedIn = containedIn;
            _avgPointSet = false;
            _avgLatitude = 0;
            _avgLongitude = 0;

            nodeArrayBounds = new NodeArrayBounds(nodes);
        }

        public Node[] Nodes { get; }

        /// <summary>
        /// If this Way is in a single Relation, mark the relation.
        /// 
        /// Note that we duplicate ways in more than one relation. They can only be in one.
        /// </summary>
        public Relation ContainedIn { get; }

        private readonly NodeArrayBounds nodeArrayBounds;

        // Memoized variables.
        private bool _avgPointSet;
        private double _avgLatitude;
        private double _avgLongitude;

        [JsonProperty("location")]
        public Point Location
        {
            get
            {
                if (!_avgPointSet)
                {
                    SetAverages();
                }
                return new Point(_avgLongitude, _avgLatitude);
            }
        }

        [JsonIgnore]
        public (Point, Point) Bounds => this.nodeArrayBounds.Bounds;

        public override string ToString()
        {
            return $"https://www.openstreetmap.org/way/{Id}";
        }


        private void SetAverages()
        {
            double sumLat = 0.0;
            double sumLng = 0.0;
            foreach (var node in Nodes)
            {
                sumLat += node.Latitude;
                sumLng += node.Longitude;
            }

            _avgLatitude = sumLat / Nodes.Length;
            _avgLongitude = sumLng / Nodes.Length;
            _avgPointSet = true;

        }
    }
}
