namespace MinimalApi.Services.Email;

public class SmtpSettings
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 587;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string FromEmail { get; set; } = "noreply@funkoapi.com";
    public string FromName { get; set; } = "FunkoApp";
    public string AdminEmail { get; set; } = "admin@funkoapi.com";
    public string Security { get; set; } = "Auto";
}