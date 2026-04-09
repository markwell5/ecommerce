FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /src

COPY domain/Ecommerce.Events/Ecommerce.Events.csproj domain/Ecommerce.Events/
COPY domain/Ecommerce.Model/Ecommerce.Model.csproj domain/Ecommerce.Model/
COPY shared/Ecommerce.Shared.Infrastructure/Ecommerce.Shared.Infrastructure.csproj shared/Ecommerce.Shared.Infrastructure/
COPY shared/Ecommerce.Shared.Protos/Ecommerce.Shared.Protos.csproj shared/Ecommerce.Shared.Protos/
COPY giftcard-service/GiftCard.Application/GiftCard.Application.csproj giftcard-service/GiftCard.Application/
COPY giftcard-service/GiftCard.Infrastructure/GiftCard.Infrastructure.csproj giftcard-service/GiftCard.Infrastructure/
COPY giftcard-service/GiftCard.Service/GiftCard.Service.csproj giftcard-service/GiftCard.Service/
RUN dotnet restore giftcard-service/GiftCard.Service/GiftCard.Service.csproj

COPY domain/ domain/
COPY shared/ shared/
COPY giftcard-service/ giftcard-service/
RUN dotnet publish giftcard-service/GiftCard.Service/GiftCard.Service.csproj -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 8080
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "GiftCard.Service.dll"]
