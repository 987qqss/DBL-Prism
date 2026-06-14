namespace Core.Models
{
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public UserType Type { get; set; }
        public string Password { get; set; } = string.Empty;
    }

    public enum UserType
    {
         Operator = 0,
         Admin = 1
    }
}