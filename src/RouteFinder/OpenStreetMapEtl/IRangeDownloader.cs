using OpenStreetMapEtl.Utils;

namespace OpenStreetMapEtl
{
    public interface IRangeDownloader
    {
        TmpFileWrapper GetRange(double westLng, double eastLng, double southLat, double northLat);
    }
}