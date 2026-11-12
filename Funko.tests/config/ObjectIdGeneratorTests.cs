
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using NUnit.Framework;

[TestFixture]
public class ObjectIdAttributeTests
{
    private ObjectIdAttribute _attribute = null!;

    [SetUp]
    public void SetUp()
    {
        _attribute = new ObjectIdAttribute();
    }

    [Test]
    public void IsValid_ValidObjectId_ReturnsTrue()
    {
        // Arrange
        var id = ObjectId.GenerateNewId().ToString();

        // Act
        var result = _attribute.IsValid(id);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void IsValid_InvalidObjectId_ReturnsFalse()
    {
        // Arrange
        var id = "12345";

        // Act
        var result = _attribute.IsValid(id);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void IsValid_NonStringValue_ReturnsFalse()
    {
        // Arrange
        var value = 12345;

        // Act
        var result = _attribute.IsValid(value);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void IsValid_NullValue_ReturnsFalse()
    {
        // Arrange
        object? value = null;

        // Act
        var result = _attribute.IsValid(value);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void FormatErrorMessage_ReturnsExpectedMessage()
    {
        // Arrange
        var name = "Id";

        // Act
        var result = _attribute.FormatErrorMessage(name);

        // Assert
        Assert.That(
            result,
            Is.EqualTo("Id debe ser un ObjectId válido de MongoDB."));
    }
}
