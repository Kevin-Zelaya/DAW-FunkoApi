using MinimalApi.models;

public class PriceHistoryType : ObjectType<PriceHistoryDto>
{
    protected override void Configure(IObjectTypeDescriptor<PriceHistoryDto> descriptor)
    {
        /// <summary> Descripción del historial de precios </summary>
        descriptor.Name("PriceHistory");
        descriptor.Description("Historial de precios de un Funko");

        /// <summary> Descripción del ID del Funko </summary>
        descriptor.Field(p => p.FunkoId)
            .Type<NonNullType<IdType>>()
            .Description("ID del Funko");

        /// <summary> Descripción de la fecha </summary>
        descriptor.Field(p => p.Date)
            .Type<NonNullType<DateTimeType>>()
            .Description("Fecha del registro del precio");

        /// <summary> Descripción del precio actual </summary>
        descriptor.Field(p => p.Price)
            .Type<NonNullType<DecimalType>>()
            .Description("Precio actual del Funko");

        /// <summary> Descripción del precio anterior </summary>
        descriptor.Field(p => p.OldPrice)
            .Type<NonNullType<DecimalType>>()
            .Description("Precio anterior del Funko");
    }
}