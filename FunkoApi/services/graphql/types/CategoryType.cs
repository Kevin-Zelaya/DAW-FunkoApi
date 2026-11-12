public class CategoryType : ObjectType<Category>
{
    protected override void Configure(IObjectTypeDescriptor<Category> descriptor)
    {
        /// <summary> Descripción de la categoría </summary>
        descriptor.Name("Categoria");
        descriptor.Description("Categoria de productos");
        /// <summary> Descripción del id </summary>
        descriptor.Field(c => c.Id)
            .Type<NonNullType<IdType>>()
            .Description("ID unico de la categoria");
        /// <summary> Descripción del nombre </summary>
        descriptor.Field(c => c.Name)
            .Type<NonNullType<StringType>>()
            .Description("Nombre de la categoria");
        /// <summary> Descripción de si está eliminado </summary>
        descriptor.Field(c => c.IsDelete)
            .Type<NonNullType<BooleanType>>()
            .Description("Indica si esta eliminado");
        /// <summary> Descripción de la fecha de creación </summary>
        descriptor.Field(c => c.CreatedAt)
            .Type<NonNullType<DateTimeType>>()
            .Description("Fecha de creacion");
        /// <summary> Descripción de la fecha de actualización </summary>
        descriptor.Field(c => c.UpdatedAt)
            .Type<DateTimeType>()
            .Description("Fecha de ultima actualizacion");
    }
}