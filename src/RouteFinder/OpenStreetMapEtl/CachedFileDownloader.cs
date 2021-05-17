using OpenStreetMapEtl.Utils;

namespace OpenStreetMapEtl
{
    public class CachedFileDownloader : IRangeDownloader
    {
        private string _filePath;

        public CachedFileDownloader(string filePath)
        {
            _filePath = filePath;
        }


        public TmpFileWrapper GetRange(BoundingBox box)
        {
            return new TmpFileWrapper(_filePath);
        }
    }
}
