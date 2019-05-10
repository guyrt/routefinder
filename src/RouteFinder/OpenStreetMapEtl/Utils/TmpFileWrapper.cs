using System;
using System.IO;

namespace OpenStreetMapEtl.Utils
{
    public class TmpFileWrapper : IDisposable
    {
        public string TmpFile { get; }

        private bool _deleteOnDispose;

        public TmpFileWrapper()
        {
            TmpFile = Path.GetTempFileName();
            Console.WriteLine(TmpFile);
            _deleteOnDispose = false;
        }

        public TmpFileWrapper(string debugPath)
        {
            TmpFile = debugPath;
            _deleteOnDispose = true;
        }

        public void Dispose()
        {
            if (_deleteOnDispose && File.Exists(TmpFile))
            {
                File.Delete(TmpFile);
            }
        }
    }
}
