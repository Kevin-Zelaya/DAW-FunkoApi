using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Supplier
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id {get; set;} = null!;
    public string Name {get; set;} = string.Empty;
    public bool IsDeleted {get; set;}
    public DateTime CreateAt {get; set;}
    public DateTime? DeleteAt {get; set;}
    public DateTime? UpdateAt {get; set;}
    public ICollection<Funko> Funkos = []; 
    // Control de concurrencia
    // public uint RowVersion {get; set;}
    public uint RowVersion {get; set;}
}