using RouteFinderDataModel;
using System;

namespace RouteCleaner
{
    public class ReasonablyEnjoyableRunningCost : IGraphCoster
    {
        /// <summary>
        /// Cost is defined by starting with a maximum value, then subtracting a fixed value for various facts about a way. 
        /// </summary>
        /// <param name="way"></param>
        /// <returns></returns>
        public double Cost(Way way, double distance)
        {
            double cost = 1.0;

            cost += Highway(way);
            cost += Lanes(way);

            return cost * distance;
        }

        private double Lanes(Way way)
        {
            if (!way.Tags.ContainsKey("lanes"))
            {
                return 0;
            }
            return 40 * (Convert.ToInt32(way.Tags["lanes"]) - 1); // subtract 40 if you have to run beside many lanes.
        }

        private double Highway(Way way)
        {
            if (!way.Tags.ContainsKey("highway"))
            {
                return 0;
            }

            var value = way.Tags["highway"];
            if (value.EndsWith("_link"))
            {
                return 99;
            }
            switch (value) {
                case "motorway": return 100;
                case "trunk": return 99;
                case "primary": return 99;
                case "secondary": return 75;
                case "tertiary":
                case "unclassified":
                case "residential": return 25;
                case "bus_guideway": return 100;
                case "escape": return 100;
                // pedestrian things    
                case "living_street":
                case "pedestrian":
                case "track":
                case "steps":
                case "path":
                case "footway": return 0;
                case "bridleway": return 1;
                default: return 10;
            }
        }
    }
}
