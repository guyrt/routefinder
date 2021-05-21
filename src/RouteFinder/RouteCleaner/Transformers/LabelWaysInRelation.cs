namespace RouteCleaner.Transformers
{
    using System.Collections.Generic;
    using System.Linq;
    using RouteFinderDataModel;
    using RouteCleaner.PolygonUtils;

    public class LabelWaysInRelation
    {
        public List<Way> Transform(Relation target, Geometry geometry)
        {
            var polygon = RelationPolygonMemoizer.Instance.GetPolygons(target).First();  // todo first is a problem. some relations have more than one! 
            var containment = new PolygonContainment(polygon);
            var ways = new List<Way>();
            foreach (var way in geometry.Ways)
            {
                var (contains, notContains) = containment.SplitWayByContainment(way);
                foreach (var newWay in contains)
                {
                    newWay.Tags.Add("rfInPolygon", newWay.FootTraffic() ? "in" : "notfoot");
                }
                foreach (var newWay in notContains)
                {
                    newWay.Tags.Add("rfInPolygon", "out");
                }

                ways.AddRange(contains);
                ways.AddRange(notContains);
            }

            return ways;
        }
    }
}
