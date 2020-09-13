using RouteFinderDataModel;
using System;
using System.Linq;
using System.Collections.Generic;

namespace RouteFinderCmd
{
    internal class SummarizeTags
    {
        /// <summary>
        /// Show key-value pairs for all tags.
        /// </summary>
        public static void Summarize(IEnumerable<Dictionary<string, string>> tags)
        {
            var waySingleTags = new Dictionary<Tuple<string, string>, int>();
            var keyUniqueValueCounter = new Dictionary<string, HashSet<string>>();
            var keyOccurenceCounter = new Dictionary<string, int>();
            foreach (var tagDict in tags) {
                foreach (var kvp in tagDict) {
                    var newKey = new Tuple<string, string>(kvp.Key, kvp.Value);
                    if (!keyUniqueValueCounter.ContainsKey(kvp.Key)) {
                        keyUniqueValueCounter.Add(kvp.Key, new HashSet<string>());
                        keyOccurenceCounter.Add(kvp.Key, 0);
                    }
                    if (!waySingleTags.ContainsKey(newKey)) {
                        waySingleTags.Add(newKey, 0);
                    }
                    waySingleTags[newKey]++;
                    keyUniqueValueCounter[kvp.Key].Add(kvp.Value);
                    keyOccurenceCounter[kvp.Key]++;
                }
            }
            foreach (var kvp in waySingleTags.Where(k => k.Value > 2)) {
                Console.WriteLine($"{kvp.Key.Item1} -- {kvp.Key.Item2}: {kvp.Value}");
            }

            foreach(var kvp in keyUniqueValueCounter) {
                Console.WriteLine($"UniqueValues\t{kvp.Key}\t{kvp.Value.Count}\t{keyOccurenceCounter[kvp.Key]}");
            }
        }
    }
}
