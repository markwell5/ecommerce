FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /src

COPY domain/Ecommerce.Events/Ecommerce.Events.csproj domain/Ecommerce.Events/
COPY domain/Ecommerce.Model/Ecommerce.Model.csproj domain/Ecommerce.Model/
COPY shared/Ecommerce.Shared.Infrastructure/Ecommerce.Shared.Infrastructure.csproj shared/Ecommerce.Shared.Infrastructure/
COPY shared/Ecommerce.Shared.Protos/Ecommerce.Shared.Protos.csproj shared/Ecommerce.Shared.Protos/
COPY wishlist-service/Wishlist.Application/Wishlist.Application.csproj wishlist-service/Wishlist.Application/
COPY wishlist-service/Wishlist.Infrastructure/Wishlist.Infrastructure.csproj wishlist-service/Wishlist.Infrastructure/
COPY wishlist-service/Wishlist.Service/Wishlist.Service.csproj wishlist-service/Wishlist.Service/
RUN dotnet restore wishlist-service/Wishlist.Service/Wishlist.Service.csproj

COPY domain/ domain/
COPY shared/ shared/
COPY wishlist-service/ wishlist-service/
RUN dotnet publish wishlist-service/Wishlist.Service/Wishlist.Service.csproj -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 8080
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "Wishlist.Service.dll"]
