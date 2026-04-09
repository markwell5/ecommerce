FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /src

COPY domain/Ecommerce.Events/Ecommerce.Events.csproj domain/Ecommerce.Events/
COPY domain/Ecommerce.Model/Ecommerce.Model.csproj domain/Ecommerce.Model/
COPY shared/Ecommerce.Shared.Infrastructure/Ecommerce.Shared.Infrastructure.csproj shared/Ecommerce.Shared.Infrastructure/
COPY shared/Ecommerce.Shared.Protos/Ecommerce.Shared.Protos.csproj shared/Ecommerce.Shared.Protos/
COPY subscription-service/Subscription.Application/Subscription.Application.csproj subscription-service/Subscription.Application/
COPY subscription-service/Subscription.Infrastructure/Subscription.Infrastructure.csproj subscription-service/Subscription.Infrastructure/
COPY subscription-service/Subscription.Service/Subscription.Service.csproj subscription-service/Subscription.Service/
RUN dotnet restore subscription-service/Subscription.Service/Subscription.Service.csproj

COPY domain/ domain/
COPY shared/ shared/
COPY subscription-service/ subscription-service/
RUN dotnet publish subscription-service/Subscription.Service/Subscription.Service.csproj -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 8080
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "Subscription.Service.dll"]
