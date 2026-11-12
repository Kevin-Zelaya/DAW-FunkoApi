namespace MinimalApi.models;

public class PriceHistoryDto
{
    public long FunkoId { get; set; }
    public DateTime Date { get; set; }
    public decimal Price { get; set; }
    public decimal OldPrice { get; set; }
}