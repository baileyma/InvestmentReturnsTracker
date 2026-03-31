using InvTracker.Models;
using Microsoft.AspNetCore.Identity;

namespace InvTracker.Authentication;

public class ClientUser : IdentityUser
{
    public bool AdminUser { get; set; }

    
}
