FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /src

COPY data-seeder/DataSeeder/DataSeeder.csproj data-seeder/DataSeeder/
RUN dotnet restore data-seeder/DataSeeder/DataSeeder.csproj

COPY data-seeder/ data-seeder/
RUN dotnet publish data-seeder/DataSeeder/DataSeeder.csproj -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "DataSeeder.dll"]
