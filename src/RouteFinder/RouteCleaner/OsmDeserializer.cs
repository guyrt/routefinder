using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using RouteCleaner.Model;

namespace RouteCleaner
{
    public class OsmDeserializer
    {
        public Geometry ReadFile(StreamReader stream)
        {
            var root = XElement.Load(stream);
            var nodes = root.Descendants("node").ToDictionary(n => n.Attribute("id")?.Value, ReadNode);
            var ways = root.Descendants("way").ToDictionary(w => w.Attribute("id")?.Value, w => ReadWay(w, nodes));
            var relations = root.Descendants("relation").Select(r => ReadRelation(r, ways)).Where(n => n != null);

            return new Geometry(nodes.Values.ToArray(), ways.Values.ToArray(), relations.ToArray());
        }

        private Relation ReadRelation(XElement relation, IReadOnlyDictionary<string, Way> ways)
        {
            if (relation.Name != "relation")
            {
                throw new InvalidOperationException($"Expected relation but got {relation}");
            }

            if (relation.Descendants("member").Select(m => m.Attribute("type")?.Value).Any(v => v == "relation"))
            {
                return null;
            }

            var memberRefs = relation.Descendants("member").Select(nd => nd.Attribute("ref")?.Value);
            var foundWays = new List<Way>();
            var incomplete = false;
            foreach (var memberRef in memberRefs)
            {
                if (!ways.ContainsKey(memberRef))
                {
                    incomplete = true;
                }
                else
                {
                    foundWays.Add(ways[memberRef]);
                }
            }

            var tags = GetTags(relation);

            return new Relation(GetId(relation), foundWays.ToArray(), tags, incomplete);
        }

        private Node ReadNode(XElement node)
        {
            if (node.Name != "node")
            {
                throw new InvalidOperationException($"Expected node but got {node}");
            }

            var lat = double.Parse(node.Attribute("lat")?.Value);
            var lng = double.Parse(node.Attribute("lon")?.Value);
            return new Node(GetId(node), lat, lng, GetTags(node));
        }

        private Way ReadWay(XElement way, Dictionary<string, Node> nodes)
        {
            if (way.Name != "way")
            {
                throw new InvalidOperationException($"Expected way but got {way}");
            }

            Node[] nodeObjs;

            var nodeRefs = way.Descendants("nd").Select(nd => nd.Attribute("ref")?.Value);
            try
            {
                nodeObjs = nodeRefs.Select(nd => nodes[nd]).ToArray();
            }
            catch (KeyNotFoundException k)
            {
                throw new KeyNotFoundException($"Key {k} not found in node list.");
            }

            var tags = GetTags(way);

            return new Way(GetId(way), nodeObjs, tags);
        }

        private Dictionary<string, string> GetTags(XElement elt)
        {
            return elt.Descendants("tag").ToDictionary(t => t.Attribute("k")?.Value, t => t.Attribute("v")?.Value);
        }

        private string GetId(XElement elt)
        {
            return elt.Attribute("id")?.Value;
        }
    }
}
