FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /src

COPY domain/Ecommerce.Events/Ecommerce.Events.csproj domain/Ecommerce.Events/
COPY domain/Ecommerce.Model/Ecommerce.Model.csproj domain/Ecommerce.Model/
COPY shared/Ecommerce.Shared.Infrastructure/Ecommerce.Shared.Infrastructure.csproj shared/Ecommerce.Shared.Infrastructure/
COPY order-service/Order.Application/Order.Application.csproj order-service/Order.Application/
COPY order-service/Order.Infrastructure/Order.Infrastructure.csproj order-service/Order.Infrastructure/
COPY order-service/Order.Service/Order.Service.csproj order-service/Order.Service/
RUN dotnet restore order-service/Order.Service/Order.Service.csproj

COPY domain/ domain/
COPY shared/ shared/
COPY order-service/ order-service/
RUN dotnet publish order-service/Order.Service/Order.Service.csproj -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 8080
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "Order.Service.dll"]
