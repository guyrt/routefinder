using RouteCleaner.Model;
using System.Collections.Generic;
using System.Linq;

namespace RouteCleaner
{
    /// <summary>
    /// Identify any trails that intersection parking lots.
    /// </summary>
    public class PathParkingLotIntersection
    {
        public Dictionary<Way, List<Way>> FindParkingLotsWithIntersections(Geometry geometry)
        {
            var parkingTrailIntersections = new Dictionary<Way, List<Way>>();

            var parkingLotNodes = geometry.GetParkingLotNodes();

            foreach (var foottraffic in geometry.Ways.Where(w => w.FootTraffic()))
            {
                foreach (var node in new []{ foottraffic.Nodes.First(), foottraffic.Nodes.Last()})
                {
                    if (parkingLotNodes.ContainsKey(node))
                    {
                        var lot = parkingLotNodes[node];
                        if (!parkingTrailIntersections.ContainsKey(lot))
                        {
                            parkingTrailIntersections.Add(lot, new List<Way>());
                        }
                        parkingTrailIntersections[lot].Add(foottraffic);
                        break;
                    }
                }
            }

            return parkingTrailIntersections;
        }
    }
}
