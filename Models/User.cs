
using Microsoft.EntityFrameworkCore;

namespace form_backend
{
    public class User
    {
        public int ID { get; set; } 
        public string Email { get; set; }
        public string Salt { get; set; }
        public string Hash { get; set; }
        public bool IsAdmin { get; set; }
        public DateTime SubmitTime { get; set; }
    }
}