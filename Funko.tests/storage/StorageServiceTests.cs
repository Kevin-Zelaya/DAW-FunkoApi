using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MinimalApi.Storage;
using MinimalApi.Errors;

namespace MinimalApi.Tests.Storage;

public class StorageServiceTests : IDisposable
{
    private readonly string _testRootPath;
    private readonly Mock<IWebHostEnvironment> _envMock;
    private readonly Mock<IConfiguration> _configMock;
    private readonly Mock<ILogger<StorageService>> _loggerMock;
    private readonly StorageService _storageService;

    public StorageServiceTests()
    {
        // Crear un directorio temporal único para los tests para no ensuciar el proyecto
        _testRootPath = Path.Combine(Path.GetTempPath(), "StorageTests_" + Guid.NewGuid());
        Directory.CreateDirectory(_testRootPath);

        //  Mockear el entorno web para que apunte a la carpeta temporal
        _envMock = new Mock<IWebHostEnvironment>();
        _envMock.Setup(e => e.WebRootPath).Returns(_testRootPath);

        // Mockear la configuración (appsettings)
        _configMock = new Mock<IConfiguration>();
        _configMock.Setup(c => c["Storage:UploadPath"]).Returns("uploads");
        _configMock.Setup(c => c.GetValue<long>("Storage:MaxFileSize", It.IsAny<long>())).Returns(1024 * 1024); // 1 MB

        // Mockear las extensiones permitidas
        var mockExtSection = new Mock<IConfigurationSection>();
        mockExtSection.Setup(s => s.Get<string[]>()).Returns([".jpg", ".png", ".jpeg"]);
        _configMock.Setup(c => c.GetSection("Storage:AllowedExtensions")).Returns(mockExtSection.Object);

        // Mockear los Content-Types permitidos
        var mockCtSection = new Mock<IConfigurationSection>();
        mockCtSection.Setup(s => s.Get<string[]>()).Returns(["image/jpeg", "image/png"]);
        _configMock.Setup(c => c.GetSection("Storage:AllowedContentTypes")).Returns(mockCtSection.Object);

        _loggerMock = new Mock<ILogger<StorageService>>();

        _storageService = new StorageService(_configMock.Object, _loggerMock.Object, _envMock.Object);
    }

    [Fact]
    public async Task SaveFileAsync_ConArchivoValido_GuardaCorrectamenteYRetornaRutaRelativa()
    {
        // Crear un IFormFile falso con contenido simulado
        var fileMock = new Mock<IFormFile>();
        var content = "fake image binary content";
        var ms = new MemoryStream();
        var writer = new StreamWriter(ms);
        writer.Write(content);
        writer.Flush();
        ms.Position = 0;

        fileMock.Setup(f => f.FileName).Returns("funko_batman.jpg");
        fileMock.Setup(f => f.Length).Returns(ms.Length);
        fileMock.Setup(f => f.ContentType).Returns("image/jpeg");
        fileMock.Setup(f => f.CopyTo(It.IsAny<Stream>()))
            .Callback<Stream>(stream => ms.CopyTo(stream));

        // Intentar guardar el archivo en la carpeta "funkos"
        var result = await _storageService.SaveFileAsync(fileMock.Object, "funkos");

        // Comprobar que fue exitoso y el archivo existe físicamente
        Assert.True(result.IsSuccess);
        Assert.Contains("/uploads/funkos/", result.Value);
        Assert.True(_storageService.FileExists(result.Value));
    }

    [Fact]
    public async Task SaveFileAsync_ConArchivoVacio_RetornaFallo()
    {
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(0);

        var result = await _storageService.SaveFileAsync(fileMock.Object, "funkos");

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task SaveFileAsync_ConExtensionNoPermitida_RetornaFallo()
    {
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns("documento.pdf");
        fileMock.Setup(f => f.Length).Returns(100);
        fileMock.Setup(f => f.ContentType).Returns("application/pdf");

        var result = await _storageService.SaveFileAsync(fileMock.Object, "funkos");

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task DeleteFileAsync_ConArchivoExistente_LoEliminaCorrectamente()
    {
        // Arrange: Primero guardamos un archivo para asegurarnos de que existe
        var fileMock = new Mock<IFormFile>();
        var ms = new MemoryStream([1, 2, 3, 4]);
        fileMock.Setup(f => f.FileName).Returns("test.png");
        fileMock.Setup(f => f.Length).Returns(4);
        fileMock.Setup(f => f.ContentType).Returns("image/png");
        fileMock.Setup(f => f.CopyTo(It.IsAny<Stream>())).Callback<Stream>(s => ms.CopyTo(s));

        var saveResult = await _storageService.SaveFileAsync(fileMock.Object, "categorias");
        var relativePath = saveResult.Value;

        var deleteResult = await _storageService.DeleteFileAsync(relativePath);

        Assert.True(deleteResult.IsSuccess);
        Assert.False(_storageService.FileExists(relativePath));
    }

    public void Dispose()
    {
        // Limpieza automática del directorio temporal tras cada test
        if (Directory.Exists(_testRootPath))
        {
            Directory.Delete(_testRootPath, true);
        }
    }
}