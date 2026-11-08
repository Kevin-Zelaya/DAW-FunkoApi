using Serilog;
using ISession = Cassandra.ISession;
using MinimalApi.config;
using Microsoft.Extensions.Options;
using MinimalApi.Services.Email;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

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


// ======= Registrando contoladores ========== 
builder.Services.AddControllers(options =>
{
    options.RespectBrowserAcceptHeader = true;
    options.ReturnHttpNotAcceptable = true;
})
.AddXmlDataContractSerializerFormatters();


// proximamente validadores
// ======= Servicios de email ==========
builder.Services.AddSingleton<EmailQueueService>();

builder.Services.AddScoped<IEmailService, MemoryEmailService>();

builder.Services.AddHostedService<EmailBackgroundWorker>();

// ======= Infraestructura ==========
builder.Services
    .AddRepositories()
    .AddDatabases(builder.Configuration)
    .AddRedisConnection(builder.Configuration)
    .AddCassandra(builder.Configuration)
    .AddServices()
    .AddAuthenticationJwt(builder.Configuration)
    .AddStorage();



// ======= MIDLEWARES ========== 
var app = builder.Build();
var apiConfig = app.Services
    .GetRequiredService<IOptions<ApiConfig>>()
    .Value;

    
app.UseHttpsRedirection();

// inicializar datos en desarrollos
if (apiConfig.shouldRecreateDb &&
    app.Environment.IsDevelopment())
{
    await app.InitializationDatabaseAsync();
    var settings = app.Services.GetRequiredService<CassandraSettings>();
    await app.Services.GetRequiredService<ISession>().InitializeSchemaAsync(settings.Keyspace);
}

app.UseStaticFiles();
app.MapOpenApi();

app.UseGlobalExceptionHandler();

// ======= AUTHENTICATION - AUTHORIZATION ========== 
app.UseAuthentication();
app.UseAuthorization();


// ======= Configuración de Swagger ==========
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "Mi API V1");
});

// ======= Buscar todos los controladores ========== 
app.MapControllers();

// Falta vaciar la bd en el caso de mongo
Console.WriteLine("======================================================");
Console.WriteLine("=================     FUNKO API     ==================");
Console.WriteLine("======================================================");
Console.WriteLine("");
Console.WriteLine("+===================+================================== ");
Console.WriteLine($"| api_environment   |      {app.Environment}    |");
Console.WriteLine($"|  swagger_route    |localhost:5205/swagger/index.html|");
Console.WriteLine($"|   Recreate_db     |                {(apiConfig.shouldRecreateDb ? "true " : "false")}            |");
Console.WriteLine($"+===================+=================================+");

app.Run();