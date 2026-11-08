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

// ======= Configuración de swgger ==========
builder.Services.AddEndpointsApiExplorer();

builder.AddCustomSwagger(); // separación de configuración de swagger
// ======= Registrando contoladores ========== 
builder.Services.AddControllers(options =>
{
    options.RespectBrowserAcceptHeader = true;
    options.ReturnHttpNotAcceptable = true;
})
.AddXmlDataContractSerializerFormatters();


// proximamente validadores
Console.WriteLine("========== CONFIGURACIÓN ==========");

Console.WriteLine($"Entorno: {builder.Environment.EnvironmentName}");

Console.WriteLine($"Keyspace directo: {builder.Configuration["DbConnections:Cassandra:Keyspace"]}");

Console.WriteLine($"Keyspace sección Cassandra: {builder.Configuration["Cassandra:Keyspace"]}");

Console.WriteLine("===================================");
// ======= Infraestructura ========== 
builder.Services
    .AddRepositories()
    .AddDatabases(builder.Configuration)
    .AddRedisConnection(builder.Configuration)
    .AddCassandra(builder.Configuration)
    .AddServices()
    .AddAuthenticationJwt(builder.Configuration)
    .AddStorage()
    .AddScoped<IEmailService, MemoryEmailService>();



// ======= MIDLEWARES ========== 
var app = builder.Build();
var apiConfig = app.Services
    .GetRequiredService<IOptions<ApiConfig>>()
    .Value;

    
app.UseHttpsRedirection();

// Configuración condicional en desarrollo
if (app.Environment.IsDevelopment())
{   
    
    // inicialización en el caso de bases de datos, redis siempre se limpia
    if (apiConfig.shouldRecreateDb)
    {
        // inicialización del servicio storage
        app.InitializeStorage(true);
        // Inicialización de mongo y postgre
        await app.InitializationDatabaseAsync();
        // Inicializació de casandra, reconstruye llaves y tablas
        var settings = app.Services.GetRequiredService<CassandraSettings>();
        await app.Services.GetRequiredService<ISession>().InitializeSchemaAsync(settings.Keyspace);
    }
}
// Permite lectura de archiovs estaticos
app.UseStaticFiles();
app.UseGlobalExceptionHandler();

// ======= AUTHENTICATION - AUTHORIZATION ========== 
app.UseAuthentication();
app.UseAuthorization();

// ======= Configuración de Swagger ==========
app.MapOpenApi();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "Mi API V1");
});

// ======= Buscar todos los controladores ========== 
app.MapControllers();
// ======= SignalR hubs ========== 
app.MapHub<FunkoHub>("/hubs/funkos").RequireAuthorization();

// Falta vaciar la bd en el caso de mongo
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