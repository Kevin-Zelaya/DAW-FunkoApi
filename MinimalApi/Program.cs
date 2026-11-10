
using Serilog;
using ISession = Cassandra.ISession;
using MinimalApi.config;
using Microsoft.Extensions.Options;
using MinimalApi.Services.Email;

var builder = WebApplication.CreateBuilder(args);

// ======= Logger de Serilog =============
Console.OutputEncoding = System.Text.Encoding.UTF8;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/productos-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

Log.Information("Iniciando aplicación...");

// ======= Configuración tipada ==========
builder.Services.Configure<ApiConfig>(
    builder.Configuration.GetSection("Api"));
builder.Services.Configure<SmtpSettings>(
    builder.Configuration.GetSection("Smtp"));

// ======= Configuración de Swagger ==========
builder.Services.AddEndpointsApiExplorer();
builder.AddCustomSwagger();

// ======= Registrando controladores ==========
builder.Services.AddControllers(options =>
{
    options.RespectBrowserAcceptHeader = true;
    options.ReturnHttpNotAcceptable = true;
});

// ======= Infraestructura ==========
builder.Services
    .AddRepositories()
    .AddDatabases(builder.Configuration)
    .AddRedisConnection(builder.Configuration)
    .AddCassandra(builder.Configuration)
    .AddServices()
    .AddAuthenticationJwt(builder.Configuration)
    .AddStorage()
    .AddSignalR();

// ======= Servicios de email ==========

// Cola de correos (Singleton)
builder.Services.AddSingleton<EmailQueueService>();

builder.Services.AddSingleton<IEmailQueueService>(
    sp => sp.GetRequiredService<EmailQueueService>());


// Worker que procesa la cola en segundo plano
builder.Services.AddHostedService<EmailBackgroundWorker>();

// ======= GraphQL ==========
builder.Services.AddGraphQL(builder.Environment);

// ======= MIDDLEWARES ==========
var app = builder.Build();

var apiConfig = app.Services
    .GetRequiredService<IOptions<ApiConfig>>()
    .Value;

// ======= Configuración de Cassandra ==========
var cassandraSettings = app.Services
    .GetRequiredService<CassandraSettings>();

var cassandraSession = app.Services
    .GetRequiredService<ISession>();

// Inicializar keyspace
await cassandraSession.InitializeKeyspaceAsync(
    cassandraSettings.Keyspace);

// Recrear base de datos si lo requiere appsettings
var recreateCassandra =
    app.Environment.IsDevelopment() && apiConfig.shouldRecreateDb;

await cassandraSession.InitializeTablesAsync(recreateCassandra);

// Inicializar bases de datos
await app.InitializeDatabasesAsync();

if (apiConfig.shouldRecreateDb)
{
    // Inicialización del servicio storage
    app.InitializeStorage(true);

    // Inicialización de Mongo y PostgreSQL
    await app.RecreateDatabasesAsync();
}

app.UseHttpsRedirection();

// Permite lectura de archivos estáticos
app.UseStaticFiles();
app.UseGlobalExceptionHandler();

// ======= AUTHENTICATION - AUTHORIZATION ==========
app.UseAuthentication();
app.UseAuthorization();

// ======= Configuración de Swagger ==========
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Funko API V1");
    });
}

// ======= Mapear endpoints de GraphQL ==========
app.MapGraphQL();

// ======= Buscar todos los controladores ==========
app.MapControllers();

// ======= SignalR hubs ==========
app.MapHub<FunkoHub>("/hubs/funkos").RequireAuthorization();

// Falta vaciar la BD en el caso de Mongo
Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("==============================================");
Console.WriteLine("              FUNKO API                      ");
Console.WriteLine("==============================================");
Console.ResetColor();

Console.WriteLine($"  Entorno:          {app.Environment.EnvironmentName}");
Console.WriteLine($"  Swagger:          http://localhost:5205/swagger");
Console.WriteLine($"  Reconstruir BD:   {(apiConfig.shouldRecreateDb ? "Sí" : "No")}");

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("==============================================");
Console.ResetColor();
Console.WriteLine();

app.Run();
