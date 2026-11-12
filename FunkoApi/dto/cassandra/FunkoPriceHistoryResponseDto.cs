using MinimalApi.models;

public record FunkoPrieceHistoryResponsesDto(
    List<PriceHistoryDto> Prices,
    decimal MaxPrice,
    decimal MinPrice
);