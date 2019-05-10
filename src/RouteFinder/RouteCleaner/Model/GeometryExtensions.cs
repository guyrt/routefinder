using System;
using System.Collections.Generic;
using System.Linq;

namespace RouteCleaner.Model
{
    static class GeometryExtensions
    {
        public static Dictionary<Node, Way> GetParkingLotNodes(this Geometry geometry)
        {
            var parkingLots = geometry.Ways.Where(w => w.IsParkingLot());
            var parkingLotNodes = new Dictionary<Node, Way>();
            foreach (var plot in parkingLots)
            {
                foreach (var node in plot.Nodes)
                {
                    parkingLotNodes.TryAdd(node, plot);
                }
            }
            return parkingLotNodes;
        }
    }
}
