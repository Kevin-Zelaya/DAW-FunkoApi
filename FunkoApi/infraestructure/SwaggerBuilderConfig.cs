using Microsoft.OpenApi;

public static class SwaggerBuilderConfig
{
    public static void AddCustomSwagger(
        this WebApplicationBuilder builder
    )
    {
        builder.Services.AddSwaggerGen(options =>
        {
            options.EnableAnnotations(); // Para activar documentación en los controladores
            options.SwaggerDoc("v1", new() // TODO: Doucmentar controladores con anotations
            {
                Title = "Funko API",
                Version = "v1",
                Description = "API REST para la gestión de Funkos con autenticación JWT, almacenamiento, bases de datos y notificaciones."
            });

            // Autenticación JWT
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Introduce tu token JWT."
            });

            // Aplicar seguridad a los endpoints
            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = []
            });
        });

    }
}