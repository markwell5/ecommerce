FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /src

COPY domain/Ecommerce.Events/Ecommerce.Events.csproj domain/Ecommerce.Events/
COPY domain/Ecommerce.Model/Ecommerce.Model.csproj domain/Ecommerce.Model/
COPY shared/Ecommerce.Shared.Infrastructure/Ecommerce.Shared.Infrastructure.csproj shared/Ecommerce.Shared.Infrastructure/
COPY shared/Ecommerce.Shared.Protos/Ecommerce.Shared.Protos.csproj shared/Ecommerce.Shared.Protos/
COPY analytics-service/Analytics.Application/Analytics.Application.csproj analytics-service/Analytics.Application/
COPY analytics-service/Analytics.Infrastructure/Analytics.Infrastructure.csproj analytics-service/Analytics.Infrastructure/
COPY analytics-service/Analytics.Service/Analytics.Service.csproj analytics-service/Analytics.Service/
RUN dotnet restore analytics-service/Analytics.Service/Analytics.Service.csproj

COPY domain/ domain/
COPY shared/ shared/
COPY analytics-service/ analytics-service/
RUN dotnet publish analytics-service/Analytics.Service/Analytics.Service.csproj -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 8080
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "Analytics.Service.dll"]
