namespace RouteCleaner.PolygonUtils
{
    using System;
    using System.Collections.Generic;
    using RouteFinderDataModel;

    /// <summary>
    /// Extracts and memoizes Polygons for <see cref="Relation"/>
    /// </summary>
    public sealed class RelationPolygonMemoizer
    {
        private static readonly Lazy<RelationPolygonMemoizer> lazy = new Lazy<RelationPolygonMemoizer>(() => new RelationPolygonMemoizer());

        public static RelationPolygonMemoizer Instance { get { return lazy.Value; } }

        private readonly Dictionary<Relation, Polygon[]> polygons;

        private RelationPolygonMemoizer()
        {
            polygons = new Dictionary<Relation, Polygon[]>();
        }

        public Polygon[] GetPolygons(Relation relation)
        {
            if (polygons.ContainsKey(relation))
            {
                return polygons[relation];
            }
            var newPolygons = BuildPolygons(relation);
            polygons.Add(relation, newPolygons);
            return newPolygons;
        }

        private Polygon[] BuildPolygons(Relation relation)
        {
            if (!relation.Tags.TryGetValue("type", out var foo) || (foo != "multipolygon" && foo != "boundary")) // multipolygon is deprecated in favor of boundary
            {
                return new Polygon[0];
            }
            try
            {
                return PolygonFactory.BuildPolygons(relation).ToArray();
            }
            catch (InvalidOperationException)
            {
                return new Polygon[0];
            }
        }

    }
}
