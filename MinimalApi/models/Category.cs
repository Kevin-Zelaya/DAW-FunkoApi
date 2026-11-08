using System.Collections.ObjectModel;

public class Category
{
    public string Id {get; set;} = string.Empty;
    public string Name {get; set;}= string.Empty;
    public string Description {get; set;} = string.Empty;
    public List<Funko> Funkos {get; set;} = new();
    public bool IsDelete {get; set;}
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeleteAT {get; set;}
    // control de concurrencia
    public uint RowVersion {get; set;}
}