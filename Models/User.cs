using System;
using System.Collections.Generic;

namespace Models
{
    public class User
    {
        public Guid UserId { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        // Navigation property
        public ICollection<Image> Images { get; set; }
    }
}
