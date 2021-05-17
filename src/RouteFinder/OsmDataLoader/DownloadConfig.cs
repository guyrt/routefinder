namespace OsmDataLoader
{
    public class DownloadConfig
    {
        public string RemoteFile { get; set; }

        public string RemoteMd5File { get; set; }

        public string LocalBz2FilePattern { get; set; }

        /// <summary>
        /// Contains pattern including spot for the md5 hash for local file.
        /// </summary>
        public string LocalFilePattern { get; set; }
    }
}
