namespace Backend.Repositories.Configuration;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = "Backend.Api";
    public string Audience { get; init; } = "Backend.Client";
    public string Key { get; init; } = "ThisIsAVeryLongDefaultDevelopmentKeyReplaceInProduction";
    public int ExpiryMinutes { get; init; } = 120;
}
