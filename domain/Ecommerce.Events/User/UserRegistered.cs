using System;

namespace Ecommerce.Events.User
{
    public class UserRegistered : EventBase
    {
        public Guid UserId { get; set; }
        public string Email { get; set; }
    }
}
