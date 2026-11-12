using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;

public class ObjectIdAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value is not string id)
            return false;

        return ObjectId.TryParse(id, out _);
    }

    public override string FormatErrorMessage(string name)
        => $"{name} debe ser un ObjectId válido de MongoDB.";
}