using Prism.Events;
using Core.Models;

namespace Core.Events
{
    public class UserLoggedInEvent : PubSubEvent<User> { }
}