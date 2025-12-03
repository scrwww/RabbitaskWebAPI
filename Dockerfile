FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

COPY ["RabbitaskWebAPI/RabbitaskWebAPI.csproj", "RabbitaskWebAPI/"]
RUN dotnet restore "RabbitaskWebAPI/RabbitaskWebAPI.csproj"

COPY ["RabbitaskWebAPI/", "RabbitaskWebAPI/"]
WORKDIR "/src/RabbitaskWebAPI"
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 5000

COPY --from=build /app/publish .
COPY healthcheck.sh /app/healthcheck.sh
RUN chmod +x /app/healthcheck.sh

ENTRYPOINT ["dotnet", "RabbitaskWebAPI.dll"]
