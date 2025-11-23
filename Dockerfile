FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

COPY RabbitaskWebAPI/*.csproj RabbitaskWebAPI/
RUN dotnet restore RabbitaskWebAPI/RabbitaskWebAPI.csproj

COPY RabbitaskWebAPI/. RabbitaskWebAPI/
RUN dotnet publish RabbitaskWebAPI -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:80

COPY --from=build /app .

# INICIAR A API
ENTRYPOINT ["dotnet", "RabbitaskWebAPI.dll"]
