using System;

namespace Models
{
    public class Image
    {
        public Guid ImageId { get; set; }
        public string FileName { get; set; }
        public string Url { get; set; }
        public DateTime UploadDate { get; set; }
        public bool IsPublic { get; set; }
        public bool IsDeleted { get; set; }

        // Foreign key
        public Guid UserId { get; set; }

        // Navigation property
        public User User { get; set; }
    }
}
