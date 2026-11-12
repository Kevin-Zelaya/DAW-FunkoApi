using HotChocolate.Types;

public class FunkoType : ObjectType<Funko>
{
    protected override void Configure(IObjectTypeDescriptor<Funko> descriptor)
    {
        /// <summary> Descripción del funko </summary>
        descriptor.Name("Funko");
        descriptor.Description("Funko y más");
        /// <summary> Descripción del id </summary>
        descriptor.Field(p => p.Id)
            .Type<NonNullType<IdType>>()
            .Description("Id único del funko");
        /// <summary> Descripción del nombre </summary>
        descriptor.Field(p => p.Name)
            .Type<NonNullType<StringType>>()
            .Description("Nombre del funko");
        /// <summary> Descripción del precio </summary>
        descriptor.Field(p => p.Price)
            .Type<NonNullType<DecimalType>>()
            .Description("Precio en euros");
        /// <summary> Descripción de la imagen </summary>
        descriptor.Field(p => p.Imagen)
            .Type<StringType>()
            .Description("URL de la imagen");
        /// <summary> Descripción de la categoria </summary>
        descriptor.Field(p => p.CategoryId)
            .Type<NonNullType<StringType>>()
            .Description("Id de la categoria");
        descriptor.Field(p => p.Category)
            .Type<CategoryType>()
            .Description("Categoria del producto");
        /// <summary> Descripción de sí está eliminado </summary>
        descriptor.Field(p => p.IsDelete)
            .Type<NonNullType<BooleanType>>()
            .Description("Indica si esta eliminado");
        /// <summary> Descripción de la fecha de creación </summary>
        descriptor.Field(p => p.CreatedAt)
            .Type<NonNullType<DateTimeType>>()
            .Description("Fecha de creacion");
        /// <summary> Descripción de la fecha de actualización </summary>
        descriptor.Field(p => p.UpdatedAt)
            .Type<DateTimeType>()
            .Description("Fecha de ultima actualizacion");
        /// <summary> Descripción del proveedor </summary>
        descriptor.Field("supplier")
            .ResolveWith<FunkoResolver>(r => r.GetSupplier(default!, default!))
            .Description("Proveedor del funko.");
        /// <summary> Descripción del proveedor </summary>
        descriptor.Field("priceHistory")
            .Type<ListType<NonNullType<PriceHistoryType>>>()
            .ResolveWith<FunkoResolver>(r => r.GetPriceHistory(default!, default!))
            .Description("Historial de precios del funko.");
        
    }
}