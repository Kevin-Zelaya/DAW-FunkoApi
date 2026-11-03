public static class AppConfig
{
    static AppConfig()
    {
        Environment = "postgre";
            
        Configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile(
                "appsettings.json",
                optional: false,
                reloadOnChange: true
            )
            .Build();
        PostgreConnectionString = Configuration["DbConnections:Postgre:ConnectionStrings:DefaultConnection"]
            ?? throw new ArgumentException("Error al obtener la propiedad ConnectionStrings:DefaultConnection");
        MongoConnectionString = Configuration["DbConnections:Mongo:ConnectionStrings:DefaultConnection"]
            ?? throw new ArgumentException("Error al obtener la propiedad ConnectionStrings:DefaultConnection");
        shouldRecreateDb = Configuration.GetValue<bool?>("shouldRecreateDb") 
            ?? throw new ArgumentException("Error al obtener la propiedad shouldRecreateDb");
        RedisConnection = Configuration["Redis:ConnectionString"]
            ?? throw new ArgumentException("Error al obtener la propiedad shouldRecreateDb");
    }

    public static IConfiguration Configuration {get; private set;}
    public static string PostgreConnectionString {get; private set;}
    public static string MongoConnectionString {get; private set;}

    public static string Environment {get; private set;}
    public static bool shouldRecreateDb {get; private set;}
    public static string RedisConnection {get; private set;}
} 