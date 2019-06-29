using OpenStreetMapEtl.Utils;
using RouteCleaner.Model;

namespace OpenStreetMapEtl
{
    public interface IFileCache
    {
        /// <summary>
        /// List all bounding boxes (corresponding to files) in the underlying cache.
        /// </summary>
        /// <returns></returns>
        BoundingBox[] ListBoxes();

        Geometry GetBox(BoundingBox box);
    }
}
