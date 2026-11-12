
namespace MinimalApi.Services.Email;

/// <summary>
/// Mensaje de correo electrónico.
/// </summary>
public class EmailMessage
{
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsHtml { get; set; } = true;

    public string? From { get; set; }
    public string? ReplyTo { get; set; }

    public List<string> Cc { get; set; } = new();
    public List<string> Bcc { get; set; } = new();

    public List<EmailAttachment> Attachments { get; set; } = new();
}

public class EmailAttachment
{
    public string FileName { get; set; } = string.Empty;
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string? ContentType { get; set; }
}

public class EmailTemplate
{
    public string Name { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string BodyHtml { get; set; } = string.Empty;
    public string? BodyText { get; set; }
}
