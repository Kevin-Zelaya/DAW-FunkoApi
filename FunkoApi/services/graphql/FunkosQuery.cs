using HotChocolate.Authorization;
using MinimalApi.models;
using MinimalApi.repositories;
[Authorize]
public class FunkosQuery
{
    [UsePaging(MaxPageSize = 50, DefaultPageSize = 10)]
    [UseFiltering]
    [UseSorting]
    public async Task<IQueryable<Funko>> GetFunkos(
        [Service] IFunkoRepository repository) 
    {
        var response =await repository.GetAllFunkosAsync();
        return response.AsQueryable();
    }
    public async Task<Funko?> GetFunko(
        long id,
        [Service] IFunkoRepository repository) 
    {
        var response =await repository.GetFunkoByIdAsync(id);
        return response;
    }
    [UsePaging]
    public async Task<IQueryable<Category>> GetCategories(
        [Service] ICategoryRepository repository) 
    {
        var response = await repository.GetAllCategoriesAsync();
        return response.AsQueryable();
    }
    [Authorize(Policy = "AdminOnly")]
    [UsePaging]
    public async Task<IQueryable<Supplier>> GetSuppliers(
        [Service] ISupplierRepository repository) 
    {
        var response = await repository.GetAllSuppliersAsync();
        return response.AsQueryable();
    }
    [Authorize(Policy = "AdminOnly")]
    [UsePaging]
    public async Task<IQueryable<PriceHistoryDto>> GetPricesHistory(
        long id,
        [Service] IFunkoPriceHistoryRepository repository) 
    {
        var response = await repository.GetHistoryByFunkoIdAsync(id);
        return response.AsQueryable();
    }
}