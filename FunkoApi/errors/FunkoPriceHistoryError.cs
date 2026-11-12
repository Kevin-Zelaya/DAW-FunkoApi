public static class FunkoPriceHistoryError
{
    public static NotFoundError NotFound(long id) => NotFoundError.FromResource("Price history", id.ToString());
}