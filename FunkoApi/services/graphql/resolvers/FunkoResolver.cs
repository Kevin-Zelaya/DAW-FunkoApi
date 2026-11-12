using MinimalApi.models;
using MinimalApi.repositories;

public class FunkoResolver
{
    public async Task<Supplier?> GetSupplier(
        [Parent] Funko funko,
        [Service] ISupplierRepository repository
    )
    {
        return await repository.GetSupplierByIdAsync(funko.SupplierId);
    }
    [UsePaging]
    public async Task<List<PriceHistoryDto>> GetPriceHistory(
        [Parent] Funko funko,
        [Service] IFunkoPriceHistoryRepository repository
    )
    {
        return await repository.GetHistoryByFunkoIdAsync(funko.Id);
    }
}