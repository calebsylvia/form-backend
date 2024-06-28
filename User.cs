
namespace form_backend
{
    public class User
    {
        public int _id { get; set; }
        public string First { get; set; }
        public string Last { get; set; }
        public string Email { get; set; }
        public DateTime DoB { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string Password { get; set; }
        public DateTime SubmitTime { get; set; }
    }
}