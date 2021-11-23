using RouteFinderDataModel;
using System.Collections.Generic;
using System.Linq;
using UserDataModel;

namespace RouteCleaner
{
    /// <summary>
    /// Track running tally of nodes in relations
    /// </summary>
    public class TrackRelationNodes
    {
        private Dictionary<string, HashSet<string>> NodesInRelations;  // relation.id -> node ids

        private Dictionary<string, HashSet<string>> WaysInRelations;  // relation.id -> way ids

        private readonly Dictionary<string, string> RelationNames; // relation.id -> relation.name

        public TrackRelationNodes(Relation[] relations)
        {
            RelationNames = relations.ToDictionary(x => x.Id, x => x.Name);
            NodesInRelations = new Dictionary<string,HashSet<string>>();
            WaysInRelations = new Dictionary<string, HashSet<string>>();
        }

        public void AddNode(Node node)
        {
            foreach (var r in node.Relations)
            {
                if (!NodesInRelations.ContainsKey(r))
                {
                    NodesInRelations.Add(r, new HashSet<string>());
                    WaysInRelations.Add(r, new HashSet<string>());
                }
                NodesInRelations[r].Add(node.Id);

                foreach (var way in node.ContainingWays)
                {
                    WaysInRelations[r].Add(way);
                }
            }
        }

        public IEnumerable<RegionSummary> GetRelationCounts()
        {
            return NodesInRelations.Select(x => new RegionSummary
            {
                RegionId = x.Key,
                NumNodesInRegion = x.Value.Count,
                NumWaysInRegion = WaysInRelations[x.Key].Count,
                RegionName = RelationNames[x.Key],
            });
        }
    }
}
