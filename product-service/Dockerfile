FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /src

COPY domain/Ecommerce.Events/Ecommerce.Events.csproj domain/Ecommerce.Events/
COPY domain/Ecommerce.Model/Ecommerce.Model.csproj domain/Ecommerce.Model/
COPY shared/Ecommerce.Shared.Infrastructure/Ecommerce.Shared.Infrastructure.csproj shared/Ecommerce.Shared.Infrastructure/
COPY shared/Ecommerce.Shared.Protos/Ecommerce.Shared.Protos.csproj shared/Ecommerce.Shared.Protos/
COPY product-service/Product.Application/Product.Application.csproj product-service/Product.Application/
COPY product-service/Product.Infrastructure/Product.Infrastructure.csproj product-service/Product.Infrastructure/
COPY product-service/Product.Service/Product.Service.csproj product-service/Product.Service/
RUN dotnet restore product-service/Product.Service/Product.Service.csproj

COPY domain/ domain/
COPY shared/ shared/
COPY product-service/ product-service/
RUN dotnet publish product-service/Product.Service/Product.Service.csproj -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 8080
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "Product.Service.dll"]
