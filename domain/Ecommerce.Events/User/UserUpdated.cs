using System;

namespace Ecommerce.Events.User
{
    public class UserUpdated : EventBase
    {
        public Guid UserId { get; set; }
    }
}
