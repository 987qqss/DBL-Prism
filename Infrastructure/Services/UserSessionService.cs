using Core.Interfaces;
using Core.Models;

namespace Infrastructure.Services
{
    public class UserSessionService : IUserSessionService
    {
        private User? _currentUser;

        public User CurrentUser
        {
            get => _currentUser ?? new User();
            set => _currentUser = value;
        }

        public bool IsLogin => _currentUser != null;

        public bool IsAdmin => _currentUser?.Type == UserType.Admin;

        public void Login(User user)
        {
            _currentUser = user;
        }

        public void Logout()
        {
            _currentUser = null;
        }
    }
}