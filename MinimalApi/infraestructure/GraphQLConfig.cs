using HotChocolate.Execution.Configuration;
using Serilog;

public static class GraphQLConfig
{
    public static IRequestExecutorBuilder AddGraphQL(
        this IServiceCollection services,
        IWebHostEnvironment environment
    )
    {
        Log.Information("Configurando GraphQL con HotChocolate...");

        return services
            .AddGraphQLServer()
            .AddAuthorization()
            .AddQueryType<FunkosQuery>()
            .AddType<FunkoType>()
            .AddType<CategoryType>()
            .AddType<SupplierType>()
            .AddType<PriceHistoryType>()
            .AddTypeExtension<FunkoResolver>()
            .AddFiltering()
            .AddSorting()
            .AddProjections()
            .ModifyRequestOptions(options =>
            {
                options.IncludeExceptionDetails = environment.IsDevelopment();
            });
    }
}