FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /src

COPY domain/Ecommerce.Events/Ecommerce.Events.csproj domain/Ecommerce.Events/
COPY domain/Ecommerce.Model/Ecommerce.Model.csproj domain/Ecommerce.Model/
COPY shared/Ecommerce.Shared.Infrastructure/Ecommerce.Shared.Infrastructure.csproj shared/Ecommerce.Shared.Infrastructure/
COPY shared/Ecommerce.Shared.Protos/Ecommerce.Shared.Protos.csproj shared/Ecommerce.Shared.Protos/
COPY stock-service/Stock.Application/Stock.Application.csproj stock-service/Stock.Application/
COPY stock-service/Stock.Infrastructure/Stock.Infrastructure.csproj stock-service/Stock.Infrastructure/
COPY stock-service/Stock.Service/Stock.Service.csproj stock-service/Stock.Service/
RUN dotnet restore stock-service/Stock.Service/Stock.Service.csproj

COPY domain/ domain/
COPY shared/ shared/
COPY stock-service/ stock-service/
RUN dotnet publish stock-service/Stock.Service/Stock.Service.csproj -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 8080
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "Stock.Service.dll"]
