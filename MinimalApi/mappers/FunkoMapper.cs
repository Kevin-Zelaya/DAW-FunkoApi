public static class FunkoMapper
{
    // =========================================================
    // Funko
    // =========================================================

    /// <summary> De dto para crear a modelo </summary>
    public static Funko ToModel(this FunkoCreateRequestDto dto) => new()
    {
        Name = dto.Name,
        CategoryId = dto.CategoryId,
        Exclusive = dto.Exclusive,
        Number = dto.Number,
        SupplierId = dto.SupplierId,
        Price = dto.Price,
        Franchise = dto.Franchise,
        Imagen = "/uploads/funkos/default.png",
    };
    /// <summary> De dto a modelo </summary>
    public static Funko ToModel(this FunkoDto dto) => new()
    {
        Name = dto.Name,
        Category = new Category
        {
            Name = dto.Category.Name,
            Description = dto.Category.Description
        },
        Exclusive = dto.Exclusive,
        Number = dto.Number,
        SupplierId = dto.Supplier.Id,
        Price = dto.Price,
        Franchise = dto.Franchise,
        RowVersion = dto.RowVersion
    };
    /// <summary> De modelo a dto </summary>
    public static FunkoDto ToDto(this Funko entity, Supplier supplier) => new FunkoDto(
        Id: entity.Id,
        Name: entity.Name,
        Category: new CategoryDto(
            Id: entity.Category!.Id!,
            Name: entity.Category.Name,
            Description: entity.Category.Description,
            RowVersion: entity.Category.RowVersion
        ),
        Exclusive: entity.Exclusive,
        Number: entity.Number,
        Price: entity.Price,
        Franchise: entity.Franchise,
        Image: entity.Imagen,
        Supplier: new SupplierDto(
            Id: supplier.Id,
            Name: supplier.Name,
            RowVersion: entity.RowVersion
        ),
        RowVersion : entity.RowVersion
    );
    public static FunkoDto ToDto(this Funko entity, SupplierDto supplier) => new FunkoDto(
        Id: entity.Id,
        Name: entity.Name,
        Category: new CategoryDto(
            Id: entity.Category!.Id,
            Name: entity.Category.Name,
            Description: entity.Category.Description,
            RowVersion: entity.Category.RowVersion
        ),
        Exclusive: entity.Exclusive,
        Number: entity.Number,
        Image: entity.Imagen,
        Price: entity.Price,
        Franchise: entity.Franchise,
        Supplier: supplier,
        RowVersion : entity.RowVersion
    );
    // =========================================================
    // Suppliers
    // =========================================================

    /// <summary> De modelo a dto </summary>
    public static SupplierDto ToDto(this Supplier entity) => new(
        Id: entity.Id,
        Name: entity.Name,
        RowVersion: entity.RowVersion
    );
    /// <summary> De modelo a dto </summary>
    public static Supplier ToEntity(this SupplierCreateRequestDto dto) => new()
    {
        Name = dto.Name,
        IsDeleted = false,
        CreateAt = DateTime.UtcNow,
        DeleteAt = null,
        UpdateAt = null
    };
    // =========================================================
    // Suppliers
    // =========================================================

    /// <summary> De modelo a dto </summary>
    public static CategoryDto ToDto(this Category category) => new(
        Id: category.Id!,
        Name: category.Name,
        Description: category.Description,
        RowVersion: category.RowVersion
    );
    /// <summary> De dto a modelo </summary>
    public static Category ToEntity(this CategoryCreateRequestDto dto) => new Category{
        Name= dto.Name,
        Description = dto.Description
    };
    /// <summary> De dto a modelo </summary>
    public static Category ToEntity(this CategoryDto dto) => new Category{
        Name= dto.Name,
        Description = dto.Description
    };

    
    
}