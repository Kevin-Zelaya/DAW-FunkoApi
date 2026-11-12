
[TestFixture]
public class YoutubeIdAttributeTests
{
    private YoutubeIdAttribute _attribute = null!;

    [SetUp]
    public void Setup()
    {
        _attribute = new YoutubeIdAttribute();
    }

    [Test]
    public void IsValid_ValidId_TrueReturned()
    {
        // Arrange
        var id = "AbCdEf123-_";

        // Act
        var response = _attribute.IsValid(id);

        // Assert
        Assert.That(response, Is.True);
    }

    [Test]
    public void IsValid_InvalidLength_FalseReturned()
    {
        // Arrange
        var id = "AbCdEf123";

        // Act
        var response = _attribute.IsValid(id);

        // Assert
        Assert.That(response, Is.False);
    }

    [Test]
    public void IsValid_InvalidCharacters_FalseReturned()
    {
        // Arrange
        var id = "AbCdEf123+/";

        // Act
        var response = _attribute.IsValid(id);

        // Assert
        Assert.That(response, Is.False);
    }

    [Test]
    public void IsValid_NullValue_FalseReturned()
    {
        // Act
        var response = _attribute.IsValid(null);

        // Assert
        Assert.That(response, Is.False);
    }

    [Test]
    public void IsValid_NonStringValue_FalseReturned()
    {
        // Act
        var response = _attribute.IsValid(12345678901);

        // Assert
        Assert.That(response, Is.False);
    }

    [Test]
    public void FormatErrorMessage_ValidName_MessageReturned()
    {
        // Arrange
        var name = "YoutubeId";

        // Act
        var response = _attribute.FormatErrorMessage(name);

        // Assert
        Assert.That(
            response,
            Is.EqualTo("YoutubeId debe ser un identificador válido de 11 caracteres.")
        );
    }
}
