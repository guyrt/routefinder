using System;
using System.Collections.Generic;
using System.Text;
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


        public TmpFileWrapper GetRange(double westLng, double eastLng, double southLat, double northLat)
        {
            return new TmpFileWrapper(_filePath);
        }
    }
}
