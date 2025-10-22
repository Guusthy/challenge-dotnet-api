using System;

namespace challenge_api_dotnet.Configs;

public static class Configuration
{
    private static string _privateKey = string.Empty;
    private static string _issuer = string.Empty;
    private static string _audience = string.Empty;

    public static string PrivateKey => _privateKey;
    public static string Issuer => _issuer;
    public static string Audience => _audience;

    public static void SetJwtOptions(string privateKey, string issuer, string audience)
    {
        if (string.IsNullOrWhiteSpace(privateKey))
        {
            throw new ArgumentException("JWT private key não pode ser vazia.", nameof(privateKey));
        }

        if (string.IsNullOrWhiteSpace(issuer))
        {
            throw new ArgumentException("JWT issuer não pode ser vazio.", nameof(issuer));
        }

        if (string.IsNullOrWhiteSpace(audience))
        {
            throw new ArgumentException("JWT audience não pode ser vazio.", nameof(audience));
        }

        _privateKey = privateKey;
        _issuer = issuer;
        _audience = audience;
    }
}
