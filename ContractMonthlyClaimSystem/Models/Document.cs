using System;

namespace CMCS.Models
{
    public class Document
    {
        public int DocumentId { get; set; }
        public int ClaimId { get; set; }

        public string FileName { get; set; }
        public string FilePath { get; set; }
        public DateTime UploadDate { get; set; }
        public long FileSize { get; set; }

        // Navigation
        public Claim Claim { get; set; }
    }
}
