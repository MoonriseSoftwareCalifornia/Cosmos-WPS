namespace Cosmos.BlobService.Models
{
    public class FileUploadMetaData
    {
        public string UploadUid { get; set; }
        public string FileName { get; set; }
        public string RelativePath { get; set; }
        public string ContentType { get; set; }
        public long ChunkIndex { get; set; }
        public long TotalChunks { get; set; }
        public long TotalFileSize { get; set; }
    }
}