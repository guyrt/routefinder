namespace RouteFinderDataModel.Tools
{
    using System.Collections.Generic;

    /// <summary>
    /// A lazy tracker of Node[] bounds
    /// </summary>
    public class NodeArrayBounds
    {
        private readonly IEnumerable<Node> nodes;

        private bool _boundsSet;
        private double _minLatitude = double.MaxValue;
        private double _maxLatitude = double.MinValue;
        private double _minLongitude = double.MaxValue;
        private double _maxLongitude = double.MinValue;

        public NodeArrayBounds(IEnumerable<Node> nodes)
        {
            this.nodes = nodes;
        }

        public (double minLng, double minLat, double maxLng, double maxLat) Bounds
        {
            get
            {
                if (!_boundsSet)
                {
                    foreach (var node in this.nodes)
                    {
                        if (node.Longitude < _minLongitude)
                        {
                            _minLongitude = node.Longitude;
                        }
                        if (node.Longitude > _maxLongitude)
                        {
                            _maxLongitude = node.Longitude;
                        }
                        if (node.Latitude < _minLatitude)
                        {
                            _minLatitude = node.Latitude;
                        }
                        if (node.Latitude > _maxLatitude)
                        {
                            _maxLatitude = node.Latitude;
                        }
                    }
                    _boundsSet = true;
                }
                return (minLng: _minLongitude, minLat: _minLatitude, maxLng: _maxLongitude, maxLat: _maxLatitude);
            }
        }

    }
}
