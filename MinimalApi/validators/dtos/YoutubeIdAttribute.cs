using System.ComponentModel.DataAnnotations;

public class YoutubeIdAttribute : ValidationAttribute
{
    private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";
    public override bool IsValid(object? value)
    {
        if (value is not string id || id.Length != 11)
            return false;

        return id.All(c => Chars.Contains(c));
    }

    public override string FormatErrorMessage(string name)
        => $"{name} debe ser un identificador válido de 11 caracteres.";
}