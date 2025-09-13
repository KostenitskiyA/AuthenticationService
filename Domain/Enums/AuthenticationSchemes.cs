namespace Domain.Enums;

public static class AuthenticationSchemes
{
    public const string Token = "Bearer";

    public const string RefreshToken = "Refresh";
    
    public const string Google = "Google";
    
    public const string GoogleCookie = "GoogleCookie";
}