using System.Net;
using CSharpFunctionalExtensions;

public class FileSystemStorageService : IStorageService, IScoped
{
    private readonly string _rootPath;
    private readonly string _uploadPath;
    private readonly long _maxFileSize;
    private readonly string[] _allowedExtensions;
    private readonly string[] _allowedContentTypes;
    private readonly ILogger<FileSystemStorageService> _logger;
    
    public FileSystemStorageService(IConfiguration configuration, ILogger<FileSystemStorageService> logger, IWebHostEnvironment env)
    {
        _logger = logger;

        _uploadPath = configuration["Storage:UploadPath"] ?? "uploads";
        _maxFileSize = configuration.GetValue<long>("Storage:MaxFileSize", 5 * 1024 * 1024);
        _allowedExtensions = configuration.GetSection("Storage:AllowedExtensions").Get<string[]>() ?? new[] { ".jpg", ".jpeg", ".png", ".gif" };
        _allowedContentTypes = configuration.GetSection("Storage:AllowedContentTypes").Get<string[]>() ?? new[] { "image/jpeg", "image/png", "image/gif" };

        var webRoot = env.WebRootPath ?? env.ContentRootPath;
        _rootPath = System.IO.Path.Combine(webRoot, _uploadPath);

        if (!Directory.Exists(_rootPath))
            Directory.CreateDirectory(_rootPath);

        _logger.LogInformation("Storage service inicializado en: {Path}", _rootPath);
    }
    private static string GenerateUniqueFileName(string originalFilename)
    {
        var extension = System.IO.Path.GetExtension(originalFilename).ToLowerInvariant();
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
        var uniqueId = Guid.NewGuid().ToString("N");
        var sanitizedName = System.IO.Path.GetFileNameWithoutExtension(originalFilename)
            .Replace(" ", "_")
            .Replace("-", "_");
        return $"{sanitizedName}_{timestamp}_{uniqueId}{extension}";
    }
    private UnitResult<DomainError> ValidateFile(IFormFile file)
    {
        if(file is null or { Length: 0 })
            return UnitResult.Failure<DomainError>(new DomainError("Archivo no proporcionado o vacío.", HttpStatusCode.BadRequest));
        if(file.Length > _maxFileSize)
            return UnitResult.Failure<DomainError>(new DomainError($"El archivo excede el tamaño máximo permitido de {_maxFileSize} bytes.", HttpStatusCode.BadRequest));

        var extension = System.IO.Path.GetExtension(file.FileName).ToLowerInvariant();
        if(!_allowedExtensions.Contains(extension))
            return UnitResult.Failure<DomainError>(new DomainError($"Extensión de archivo no permitida. Permitidas: {string.Join(", ", _allowedExtensions)}", HttpStatusCode.BadRequest));

        var contentType = file.ContentType?.ToLowerInvariant();
        if (contentType is null || !_allowedContentTypes.Any(ct => contentType.Contains(ct.Split('/')[1])))
            return UnitResult.Failure<DomainError>(StorageError.TipoContenidoNoPermitido());

        var filename = System.IO.Path.GetFileName(file.FileName);
        if (filename.Contains("..") || filename.Contains('/') || filename.Contains('\\'))
            return UnitResult.Failure<DomainError>(StorageError.NombreArchivoInvalido());

        return UnitResult.Success<DomainError>();
    }

    public Task<Result<String, DomainError>> SaveFileAsync(IFormFile file, string folder)
    {
        var validation  = ValidateFile(file);
        if(validation.IsFailure)
            return Task.FromResult(Result.Failure<String, DomainError>(validation.Error));
        try
        {
            var filename = GenerateUniqueFileName(file.FileName);
            var folderPath = System.IO.Path.Combine(_rootPath, folder);
            Directory.CreateDirectory(folderPath);

            var filePath = System.IO.Path.Combine(folderPath, filename);
            var relatePath = GetRelativePath(filename, folder);

            using var stream = new FileStream(filePath, FileMode.Create);
            file.CopyTo(stream);

            return Task.FromResult(Result.Success<String, DomainError>(relatePath));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error guardando archivo");
            return Task.FromResult(Result.Failure<string, DomainError>(StorageError.ErrorGuardando()));
        }

    }

    public Task<Result<bool, DomainError>> DeleteFileAsync(string filename)
    {
        if (string.IsNullOrEmpty(filename))
            return Task.FromResult(Result.Success<bool, DomainError>(true));

        try
        {
            var fullPath = GetFullPath(filename);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                _logger.LogInformation("Archivo eliminado: {Filename}", filename);
            }
            return Task.FromResult(Result.Success<bool, DomainError>(true));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error eliminando archivo {Filename}", filename);
            return Task.FromResult(Result.Failure<bool, DomainError>(StorageError.ErrorEliminando()));
        }
    }

    public bool FileExists(string filename)
    {
        if (string.IsNullOrEmpty(filename)) return false;
        return File.Exists(GetFullPath(filename));
    }

    public string GetFullPath(string filename)
    {
        if (System.IO.Path.IsPathRooted(filename)) return filename;
        var prefix = $"/{_uploadPath}/";
        if (filename.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            filename = filename[prefix.Length..];
        return System.IO.Path.Combine(_rootPath, filename);
    }

     public string GetRelativePath(string filename, string folder = "productos")
    {
        return $"/{_uploadPath}/{folder}/{filename}";
    }
}