using Core.Models;

namespace Core.Interfaces
{
    public interface IUserSessionService
    {
        User CurrentUser { get; set; }
        bool IsLogin { get; }
        bool IsAdmin { get; }
        void Login(User user);
        void Logout();
    }
}