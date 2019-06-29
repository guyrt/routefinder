using OpenStreetMapEtl.Azure;
using System.Collections.Generic;
using System.Linq;

namespace OpenStreetMapEtl.Storage
{
    public class CacheFileCleanup
    {
        /// <summary>
        /// Clean up overlapping files.
        /// </summary>
        public HashSet<string> FindRedundantFiles()
        {
            var afc = new AzureFileCache();
            var files = afc.GetFiles();
            var sortedFiles = files.OrderByDescending(f => f.Properties.Created).ToList();

            var removedFiles = new HashSet<string>();
            for (var i = 0; i < sortedFiles.Count - 1; i++)
            {
                var laterFile = sortedFiles[i];
                if (removedFiles.Contains(laterFile.Name)) { continue; }
                var laterBox = BoundingBoxFilenameConverter.ParseFileName(laterFile.Name);
                for (var j = i+1; j < sortedFiles.Count; j++)
                {
                    var earlierFile = sortedFiles[j];
                    if (removedFiles.Contains(earlierFile.Name)) { continue; }
                    var earlierBox = BoundingBoxFilenameConverter.ParseFileName(earlierFile.Name);
                    if (laterBox.Overlap(earlierBox))
                    {
                        removedFiles.Add(earlierFile.Name);
                    }
                }
            }

            return removedFiles;
        }

        public int RemoveRedundantFiles(IEnumerable<string> fileNames)
        {
            var i = 0;
            var afc = new AzureFileCache();
            foreach (var f in fileNames)
            {
                if (afc.RemoveFile(f))
                {
                    i++;
                }
            }

            return i;
        }
    }
}
