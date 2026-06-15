using Prism.Mvvm;
using Prism.Commands;
using System.Collections.ObjectModel;

namespace UserModule.ViewModels
{
    public class UserViewModel : BindableBase
    {
        private string _currentUser = "管理员";

        public string CurrentUser
        {
            get => _currentUser;
            set => SetProperty(ref _currentUser, value);
        }

        public ObservableCollection<UserItem> Users { get; } = new();

        public DelegateCommand AddUserCommand { get; }
        public DelegateCommand ModifyPasswordCommand { get; }
        public DelegateCommand LogoutCommand { get; }

        public UserViewModel()
        {
            AddUserCommand = new DelegateCommand(AddUser);
            ModifyPasswordCommand = new DelegateCommand(ModifyPassword);
            LogoutCommand = new DelegateCommand(Logout);

            InitializeMockData();
        }

        private void InitializeMockData()
        {
            Users.Add(new UserItem { UserName = "admin", Role = "管理员", Status = "在线" });
            Users.Add(new UserItem { UserName = "operator", Role = "操作员", Status = "在线" });
            Users.Add(new UserItem { UserName = "guest", Role = "访客", Status = "离线" });
            Users.Add(new UserItem { UserName = "engineer", Role = "工程师", Status = "离线" });
        }

        private void AddUser()
        {
        }

        private void ModifyPassword()
        {
        }

        private void Logout()
        {
        }
    }

    public class UserItem
    {
        public string UserName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}