
namespace form_backend
{
    public class UserService
    {
        private readonly List<User> _users = new List<User>();

        public IEnumerable<User> GetAllUsers() => _users;

        public void AddUser(User user){
            _users.Add(user);
        }

    }
}