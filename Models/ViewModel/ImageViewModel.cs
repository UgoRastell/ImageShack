using System;

namespace Models.ViewModel
{
    public class ImageViewModel
    {
        public Guid ImageId { get; set; }
        public string FileName { get; set; }
        public string Url { get; set; }
        public DateTime UploadDate { get; set; }
        public bool IsPublic { get; set; }
        public bool IsDeleted { get; set; }
        public Guid UserId { get; set; }
        public string Email { get; set; }
    }
}
