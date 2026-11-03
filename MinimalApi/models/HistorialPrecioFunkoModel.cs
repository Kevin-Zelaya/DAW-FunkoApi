namespace MinimalApi.models;

public class HistorialPrecioFunkoModel
{
    public long FunkoId { get; set; }
    public DateTimeOffset Fecha { get; set; }
    public decimal Precio { get; set; }
    public decimal PrecioAnterior { get; set; }
}