namespace InvTracker.Authentication;

public class LoginResponse
{
    public required string JwtToken { get; set; }

    public DateTime ExpirationDate { get; set; }
}
