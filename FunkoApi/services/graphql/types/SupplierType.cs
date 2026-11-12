public class SupplierType : ObjectType<Supplier>
{
    protected override void Configure(IObjectTypeDescriptor<Supplier> descriptor)
    {
        /// <summary> Descripción del proveedor </summary>
        descriptor.Name("Proveedor");
        descriptor.Description("Proveedor de funkos");
        /// <summary> Descripción del id </summary>
        descriptor.Field(c => c.Id)
            .Type<NonNullType<IdType>>()
            .Description("ID unico de la categoria");
        /// <summary> Descripción del nombre </summary>
        descriptor.Field(c => c.Name)
            .Type<NonNullType<StringType>>()
            .Description("Nombre de la categoria");
        /// <summary> Descripción de si está eliminado </summary>
        descriptor.Field(c => c.IsDeleted)
            .Type<NonNullType<BooleanType>>()
            .Description("Indica si esta eliminado");
        /// <summary> Descripción de la fecha de creación </summary>
        descriptor.Field(c => c.CreateAt)
            .Type<NonNullType<DateTimeType>>()
            .Description("Fecha de creacion");
        /// <summary> Descripción de la fecha de actualización </summary>
        descriptor.Field(c => c.UpdateAt)
            .Type<DateTimeType>()
            .Description("Fecha de ultima actualizacion");
    }
}