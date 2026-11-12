using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;

public class YoutubeIdValueGenerator : ValueGenerator<string>
{
    private const string Chars =
        "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";

    public override string Next(EntityEntry entry)
        => GenerateId();

    public static string GenerateId()
        => new string(
            Enumerable.Range(0, 11)
                .Select(_ => Chars[Random.Shared.Next(Chars.Length)])
                .ToArray());

    public override bool GeneratesTemporaryValues => false;
}