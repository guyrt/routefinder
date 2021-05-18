using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using RouteFinderDataModel;

namespace RouteCleaner
{
    public class OsmDeserializer
    {
        private readonly bool discardPartials;

        public OsmDeserializer(bool discardPartials = false)
        {
            this.discardPartials = discardPartials;
        }

        public Geometry ReadFile(StreamReader stream)
        {
            var nodes = new Dictionary<string, Node>();
            var ways = new Dictionary<string, Way>();
            var relations = new List<Relation>();

            bool hitWay = false;
            bool hitRelation = false;

            foreach (var childElt in StreamRootChildDoc(stream))
            {
                switch (childElt.Name.LocalName)
                {
                    case "node":
                        if (hitWay)
                        {
                            throw new InvalidDataException("Found node after way. This isn't allowed");
                        }
                        var node = ReadNode(childElt);
                        nodes.Add(node.Id, node);
                        break;
                    case "way":
                        if (hitRelation)
                        {
                            throw new InvalidDataException("Found way after relation. This isn't allowed");
                        }
                        if (!hitWay)
                        {
                            Console.WriteLine($"Found {nodes.Count} nodes.");
                        }
                        hitWay = true;
                        var way = ReadWay(childElt, nodes);
                        if (way != default)
                        {
                            ways.Add(way.Id, way);
                        }
                        break;
                    case "relation":
                        if (!hitRelation)
                        {
                            Console.WriteLine($"Found {ways.Count} ways.");
                        }
                        hitRelation = true;
                        var relation = ReadRelation(childElt, ways);
                        if (relation != default)
                        {
                            relations.Add(relation);
                        }
                        break;
                }
            }
            Console.WriteLine($"Found {relations.Count} relations.");
            return new Geometry(nodes.Values.ToArray(), ways.Values.ToArray(), relations.ToArray());
        }

        private IEnumerable<XElement> StreamRootChildDoc(StreamReader stream)
        {
            using (XmlReader reader = XmlReader.Create(stream))
            {
                reader.MoveToContent();
                // Parse the file and display each of the nodes.
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (reader.Name == "node")
                            {
                                XElement el = XElement.ReadFrom(reader) as XElement;
                                if (el != null)
                                    yield return el;
                            }
                            if (reader.Name == "way")
                            {
                                XElement el = XElement.ReadFrom(reader) as XElement;
                                if (el != null)
                                    yield return el;
                            }
                            if (reader.Name == "relation")
                            {
                                XElement el = XElement.ReadFrom(reader) as XElement;
                                if (el != null)
                                    yield return el;
                            }
                            break;
                    }
                }
            }
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

            var memberRefs = relation.Descendants("member").Where(nd => nd.Attribute("type")?.Value == "way").Select(nd => nd.Attribute("ref")?.Value);
            var foundWays = new List<Way>();
            var incomplete = false;
            foreach (var memberRef in memberRefs)
            {
                if (!ways.ContainsKey(memberRef))
                {
                    if (this.discardPartials)
                    {
                        return default;
                    }

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
                if (this.discardPartials)
                {
                    return default;
                }
                throw new KeyNotFoundException($"Key {k} not found in node list.", k);
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
