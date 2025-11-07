# Build
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

# Copia csproj e restaura pacotes
COPY RabbitaskWebAPI/*.csproj ./RabbitaskWebAPI/
WORKDIR /app/RabbitaskWebAPI
RUN dotnet restore

# Copia todo o restante do código
COPY RabbitaskWebAPI/. ./

# Publica a aplicação
RUN dotnet publish -c Release -o /app/publish

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "RabbitaskWebAPI.dll"]

