using System.Collections.Generic;
using System.Linq;
using RouteCleaner.Model;

namespace RouteCleaner.Transformers
{
    /// <summary>
    /// Split any way that gets intersected in its middle by another way.
    ///
    /// Functionally, assume any intersection includes the start/end node of one of the two ways.
    /// </summary>
    public class SplitBisectedWays
    {
        public List<Way> Transform(List<Way> ways)
        {
            var endNodes = new HashSet<Node>();
            foreach (var way in ways)
            {
                endNodes.Add(way.Nodes.First());
                endNodes.Add(way.Nodes.Last());
            }
            var finalWays = new List<Way>();
            foreach (var way in ways)
            {
                var cntr = 0;
                var lastStartIndex = 0;
                for (var i = 1; i < way.Nodes.Length - 1; i++)
                {
                    var node = way.Nodes[i];
                    if (endNodes.Contains(node))
                    {
                        var nodes = new Node[i - lastStartIndex + 1];
                        for (var j = lastStartIndex; j <= i; j++)
                        {
                            nodes[j - lastStartIndex] = way.Nodes[j];
                        }
                        var newWay = new Way($"{way.Id}_bisect_{cntr++}", nodes.ToArray(), new Dictionary<string, string>(way.Tags));
                        finalWays.Add(newWay);
                        lastStartIndex = i;
                    }
                }

                if (lastStartIndex > 0)
                {
                    var nodes = new Node[way.Nodes.Length - lastStartIndex];
                    for (var j = lastStartIndex; j < way.Nodes.Length; j++)
                    {
                        nodes[j - lastStartIndex] = way.Nodes[j];
                    }
                    var newWay = new Way($"{way.Id}_bisect_{cntr}", nodes.ToArray(), new Dictionary<string, string>(way.Tags));
                    finalWays.Add(newWay);
                }
                else
                {
                    finalWays.Add(way);
                }
            }

            return finalWays;
        }
    }
}
