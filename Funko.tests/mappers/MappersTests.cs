public class MappersTests
{
    
    [Test]
    public void AllMappers_InOneTest()
    {
        // Arrange
        var supplierId = "507f1f77bcf86cd799439011";
        var categoryId = "prueba123";

        var create = new FunkoCreateRequestDto(
            Name: "Spider-Man",
            Franchise: "Marvel",
            Number: 100,
            Exclusive: false,
            Price: 15.99,
            CategoryId: categoryId,
            SupplierId: supplierId
        );

        var supplier = new Supplier
        {
            Id = supplierId,
            Name = "Proveedor de prueba",
            RowVersion = 954
        };

        var supplierDto = new SupplierDto(
            Id: supplier.Id,
            Name: supplier.Name,
            RowVersion: supplier.RowVersion
        );

        var supplierCreateDto = new SupplierCreateRequestDto(
            Name: "Nuevo proveedor"
        );

        var categoryCreateDto = new CategoryCreateRequestDto(
            Name: "Nueva categoría",
            Description: "Descripción de prueba"
        );

        // Act

        // FunkoCreateRequestDto -> Funko
        var funko = create.ToModel();

        funko.Category = new Category
        {
            Id = categoryId,
            Name = "Prueba",
            Description = "Descripción de prueba",
            RowVersion = 954
        };

        // Funko -> FunkoDto (Supplier)
        var funkoDto = funko.ToDto(supplier);

        // Funko -> FunkoDto (SupplierDto)
        var funkoDtoWithSupplierDto = funko.ToDto(supplierDto);

        // Category -> CategoryDto
        var categoryDto = funko.Category.ToDto();

        // Supplier -> SupplierDto
        var mappedSupplierDto = supplier.ToDto();

        // SupplierCreateRequestDto -> Supplier
        var mappedSupplier = supplierCreateDto.ToEntity();

        // CategoryCreateRequestDto -> Category
        var mappedCategory = categoryCreateDto.ToEntity();

        // FunkoDto -> Funko
        var funkoFromDto = funkoDto.ToModel();

        // Assert

        // FunkoCreateRequestDto -> Funko
        Assert.That(funko.Name, Is.EqualTo(create.Name));
        Assert.That(funko.Franchise, Is.EqualTo(create.Franchise));
        Assert.That(funko.Number, Is.EqualTo(create.Number));
        Assert.That(funko.Exclusive, Is.EqualTo(create.Exclusive));
        Assert.That(funko.Price, Is.EqualTo(create.Price));
        Assert.That(funko.CategoryId, Is.EqualTo(create.CategoryId));
        Assert.That(funko.SupplierId, Is.EqualTo(create.SupplierId));
        Assert.That(funko.Imagen, Is.EqualTo("default.png"));

        // Funko -> FunkoDto
        Assert.That(funkoDto.Name, Is.EqualTo(funko.Name));
        Assert.That(funkoDto.Supplier.Id, Is.EqualTo(supplier.Id));
        Assert.That(funkoDto.Category.Id, Is.EqualTo(categoryId));

        // Funko -> FunkoDto con SupplierDto
        Assert.That(funkoDtoWithSupplierDto.Supplier, Is.EqualTo(supplierDto));

        // Category -> CategoryDto
        Assert.That(categoryDto.Id, Is.EqualTo(funko.Category.Id));
        Assert.That(categoryDto.Name, Is.EqualTo(funko.Category.Name));
        Assert.That(categoryDto.Description, Is.EqualTo(funko.Category.Description));
        Assert.That(categoryDto.RowVersion, Is.EqualTo(funko.Category.RowVersion));

        // Supplier -> SupplierDto
        Assert.That(mappedSupplierDto.Id, Is.EqualTo(supplier.Id));
        Assert.That(mappedSupplierDto.Name, Is.EqualTo(supplier.Name));
        Assert.That(mappedSupplierDto.RowVersion, Is.EqualTo(supplier.RowVersion));

        // SupplierCreateRequestDto -> Supplier
        Assert.That(mappedSupplier.Name, Is.EqualTo(supplierCreateDto.Name));
        Assert.That(mappedSupplier.IsDeleted, Is.False);
        Assert.That(mappedSupplier.DeleteAt, Is.Null);
        Assert.That(mappedSupplier.UpdateAt, Is.Null);
        Assert.That(mappedSupplier.CreateAt, Is.Not.EqualTo(default(DateTime)));

        // CategoryCreateRequestDto -> Category
        Assert.That(mappedCategory.Name, Is.EqualTo(categoryCreateDto.Name));
        Assert.That(mappedCategory.Description, Is.EqualTo(categoryCreateDto.Description));

        // FunkoDto -> Funko
        Assert.That(funkoFromDto.Name, Is.EqualTo(funkoDto.Name));
        Assert.That(funkoFromDto.SupplierId, Is.EqualTo(funkoDto.Supplier.Id));
        Assert.That(funkoFromDto.Price, Is.EqualTo(funkoDto.Price));
        Assert.That(funkoFromDto.RowVersion, Is.EqualTo(funkoDto.RowVersion));
    }
}