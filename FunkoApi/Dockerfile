ARG PARTE1=mcr
ARG PARTE2=microsoft
ARG PARTE3=com

FROM ${PARTE1}.${PARTE2}.${PARTE3}/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY *.csproj ./
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM ${PARTE1}.${PARTE2}.${PARTE3}/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "MinimalApi.dll"]
