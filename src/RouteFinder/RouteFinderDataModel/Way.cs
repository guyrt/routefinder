namespace RouteFinderDataModel
{
    using System.Linq;
    using Microsoft.Azure.Cosmos.Spatial;
    using Newtonsoft.Json;
    using RouteFinderDataModel.Tools;
    using System.Collections.Generic;

    public class Way : TaggableIdentifiableElement
    {
        public Way(string id, string[] nodeIds, Dictionary<string, string> tags = null, string containedIn = null, bool isComposite = false) : base(id, tags)
        {
            NodeIds = nodeIds;
            ContainedIn = containedIn;
            this.Nodes = new List<Node>();
        }

        public Way(string id, Node[] nodes, Dictionary<string, string> tags = null, string containedIn = null, bool isComposite = false) : this(id, nodes.Select(n => n.Id).ToArray(), tags, containedIn, isComposite)
        {
            this.Nodes = nodes.ToList();
            nodeArrayBounds = new NodeArrayBounds(this.Nodes);
        }

        public string[] NodeIds { get; }

        /// <summary>
        /// Ways are an optional field that can be updated with the actual nodes that correspond to NodeIds.
        /// </summary>
        [JsonIgnore]
        public List<Node> Nodes { get; }

        /// <summary>
        /// If this Way is in a single Relation, mark the relation.
        /// 
        /// Note that we duplicate ways in more than one relation. They can only be in one.
        /// </summary>
        public string ContainedIn { get; }

        /// <summary>
        /// If true then this was constructed by us and may not have contiguous nodes.
        /// </summary>
        [JsonIgnore]
        public bool IsComposite { get; }

        [JsonIgnore]
        public bool IsComplete => this.Nodes.Count == this.NodeIds.Length;

        private NodeArrayBounds nodeArrayBounds;

        /// <summary>
        /// Add a node to the nodes. This assumes you have already validated the add makes sense. In fact, if you add the right number of arbitrary
        /// nodes, the Way belives it is complete!
        /// 
        /// Returns true iff the added node completes the Way
        /// </summary>
        /// <param name="node"></param>
        public bool AddNode(Node node)
        {
            var currentIsComplete = this.IsComplete;
            this.Nodes.Add(node);
            if (this.IsComplete && !currentIsComplete)
            {
                nodeArrayBounds = new NodeArrayBounds(this.Nodes);
                return true;
            }
            return false;
        }

        [JsonIgnore]
        public (double minLng, double minLat, double maxLng, double maxLat) Bounds => this.IsComposite || !this.IsComplete ? throw new System.Exception("Can't take bounds of Composite Way") : this.nodeArrayBounds.Bounds;

        public override string ToString()
        {
            return $"https://www.openstreetmap.org/way/{Id}";
        }

        public string Name {
            get {
                var defaultName = "Unnamed way";
                if (this.Tags.TryGetValue("name", out var wayName))
                {
                    return string.IsNullOrEmpty(wayName) ? defaultName : wayName;
                }
                else
                {
                    return defaultName;
                }
            }
        }
    }
}
