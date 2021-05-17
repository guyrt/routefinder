using OpenStreetMapEtl.Utils;

namespace OpenStreetMapEtl
{
    public interface IRangeDownloader
    {
        TmpFileWrapper GetRange(BoundingBox box);
    }
}