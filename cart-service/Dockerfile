FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /src

COPY shared/Ecommerce.Shared.Infrastructure/Ecommerce.Shared.Infrastructure.csproj shared/Ecommerce.Shared.Infrastructure/
COPY shared/Ecommerce.Shared.Protos/Ecommerce.Shared.Protos.csproj shared/Ecommerce.Shared.Protos/
COPY shared/Ecommerce.Shared.GrpcClients/Ecommerce.Shared.GrpcClients.csproj shared/Ecommerce.Shared.GrpcClients/
COPY domain/Ecommerce.Events/Ecommerce.Events.csproj domain/Ecommerce.Events/
COPY domain/Ecommerce.Model/Ecommerce.Model.csproj domain/Ecommerce.Model/
COPY cart-service/Cart.Application/Cart.Application.csproj cart-service/Cart.Application/
COPY cart-service/Cart.Infrastructure/Cart.Infrastructure.csproj cart-service/Cart.Infrastructure/
COPY cart-service/Cart.Service/Cart.Service.csproj cart-service/Cart.Service/
RUN dotnet restore cart-service/Cart.Service/Cart.Service.csproj

COPY domain/ domain/
COPY shared/ shared/
COPY cart-service/ cart-service/
RUN dotnet publish cart-service/Cart.Service/Cart.Service.csproj -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 8080
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "Cart.Service.dll"]
