using System.ComponentModel.DataAnnotations;

namespace InvTracker.Authentication;

public class RegistrationModel
{
    public required string UserName { get; set; }

    public required string Password { get; set; }

    [EmailAddress]
    public required string Email { get; set; }
}
