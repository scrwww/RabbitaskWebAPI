FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["RabbitaskWebAPI/RabbitaskWebAPI.csproj", "RabbitaskWebAPI/"]
RUN dotnet restore "RabbitaskWebAPI/RabbitaskWebAPI.csproj"

COPY ["RabbitaskWebAPI/", "RabbitaskWebAPI/"]
WORKDIR "/src/RabbitaskWebAPI"
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 5000

COPY --from=build /app/publish .

HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD curl -f http://localhost:5000/health || exit 1

ENTRYPOINT ["dotnet", "RabbitaskWebAPI.dll"]
