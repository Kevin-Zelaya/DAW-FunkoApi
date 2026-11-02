using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;

[TestFixture]
public class StorageServiceTests
{
    private string _testRootPath = null!;
    private Mock<IWebHostEnvironment> _envMock = null!;
    private Mock<IConfiguration> _configMock = null!;
    private Mock<ILogger<FileSystemStorageService>> _loggerMock = null!;
    private FileSystemStorageService _storageService = null!;

    [SetUp]
    public void SetUp()
    {
        _testRootPath = Path.Combine(
            Path.GetTempPath(),
            "StorageTests_" + Guid.NewGuid()
        );

        Directory.CreateDirectory(_testRootPath);

        _envMock = new Mock<IWebHostEnvironment>();
        _envMock.Setup(e => e.WebRootPath).Returns(_testRootPath);

        // Es mejor utilizar una configuración real en memoria.
        var settings = new Dictionary<string, string?>
        {
            ["Storage:UploadPath"] = "uploads",
            ["Storage:MaxFileSize"] = "1048576",
            ["Storage:AllowedExtensions:0"] = ".jpg",
            ["Storage:AllowedExtensions:1"] = ".png",
            ["Storage:AllowedExtensions:2"] = ".jpeg",
            ["Storage:AllowedContentTypes:0"] = "image/jpeg",
            ["Storage:AllowedContentTypes:1"] = "image/png"
        };

        _configMock = new Mock<IConfiguration>();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        _loggerMock = new Mock<ILogger<FileSystemStorageService>>();

        _storageService = new FileSystemStorageService(
            configuration,
            _loggerMock.Object,
            _envMock.Object
        );
    }

    [Test]
    public async Task SaveFileAsync_ConArchivoValido_GuardaCorrectamente()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();

        var ms = new MemoryStream();
        var writer = new StreamWriter(ms);
        writer.Write("fake image binary content");
        writer.Flush();
        ms.Position = 0;

        fileMock.Setup(f => f.FileName).Returns("funko_batman.jpg");
        fileMock.Setup(f => f.Length).Returns(ms.Length);
        fileMock.Setup(f => f.ContentType).Returns("image/jpeg");

        fileMock.Setup(f => f.CopyToAsync(
            It.IsAny<Stream>(),
            It.IsAny<CancellationToken>()
        )).Returns<Stream, CancellationToken>((stream, _) =>
        {
            ms.Position = 0;
            return ms.CopyToAsync(stream);
        });

        // Act
        var result = await _storageService.SaveFileAsync(
            fileMock.Object,
            "funkos"
        );

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Does.Contain("/uploads/funkos/"));
    }

    [Test]
    public async Task SaveFileAsync_ConArchivoVacio_RetornaFallo()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(0);

        // Act
        var result = await _storageService.SaveFileAsync(
            fileMock.Object,
            "funkos"
        );

        // Assert
        Assert.That(result.IsFailure, Is.True);
    }

    [Test]
    public async Task SaveFileAsync_ConExtensionNoPermitida_RetornaFallo()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();

        fileMock.Setup(f => f.FileName).Returns("documento.pdf");
        fileMock.Setup(f => f.Length).Returns(100);
        fileMock.Setup(f => f.ContentType).Returns("application/pdf");

        // Act
        var result = await _storageService.SaveFileAsync(
            fileMock.Object,
            "funkos"
        );

        // Assert
        Assert.That(result.IsFailure, Is.True);
    }

    [Test]
    public async Task DeleteFileAsync_ConArchivoExistente_LoEliminaCorrectamente()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        var ms = new MemoryStream([1, 2, 3, 4]);

        fileMock.Setup(f => f.FileName).Returns("test.png");
        fileMock.Setup(f => f.Length).Returns(4);
        fileMock.Setup(f => f.ContentType).Returns("image/png");

        fileMock.Setup(f => f.CopyToAsync(
            It.IsAny<Stream>(),
            It.IsAny<CancellationToken>()
        )).Returns<Stream, CancellationToken>((stream, _) =>
        {
            ms.Position = 0;
            return ms.CopyToAsync(stream);
        });

        var saveResult = await _storageService.SaveFileAsync(
            fileMock.Object,
            "categorias"
        );

        Assert.That(saveResult.IsSuccess, Is.True);

        // Act
        var deleteResult = await _storageService.DeleteFileAsync(
            saveResult.Value
        );

        // Assert
        Assert.That(deleteResult.IsSuccess, Is.True);
        Assert.That(
            _storageService.FileExists(saveResult.Value),
            Is.False
        );
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_testRootPath))
            Directory.Delete(_testRootPath, true);
    }
}