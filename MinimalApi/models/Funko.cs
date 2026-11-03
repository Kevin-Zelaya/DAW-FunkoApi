public class Funko
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Franchise { get; set; } = string.Empty;
    public int Number { get; set; }
    public string SupplierId {get; set;} = string.Empty;
    public string? CategoryId {get; set;}
    public Category? Category { get; set; }
    public bool Exclusive { get; set; }
    public double Price { get; set; }
    public string? Imagen { get; set; }
    public bool IsDelete {get; set;}
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeleteAT {get; set;}
    // control de concurrencia
    public uint RowVersion {get; set;}
}
