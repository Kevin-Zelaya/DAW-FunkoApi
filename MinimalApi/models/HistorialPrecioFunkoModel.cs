namespace MinimalApi.Models;

public class HistorialPrecioFunkoModel
{
    public long FunkoId { get; set; }
    public DateTime Fecha { get; set; }
    public double Precio { get; set; }
    public double PrecioAnterior { get; set; }
}