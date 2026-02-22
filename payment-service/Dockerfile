FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /src

COPY domain/Ecommerce.Events/Ecommerce.Events.csproj domain/Ecommerce.Events/
COPY domain/Ecommerce.Model/Ecommerce.Model.csproj domain/Ecommerce.Model/
COPY shared/Ecommerce.Shared.Infrastructure/Ecommerce.Shared.Infrastructure.csproj shared/Ecommerce.Shared.Infrastructure/
COPY payment-service/Payment.Application/Payment.Application.csproj payment-service/Payment.Application/
COPY payment-service/Payment.Infrastructure/Payment.Infrastructure.csproj payment-service/Payment.Infrastructure/
COPY payment-service/Payment.Service/Payment.Service.csproj payment-service/Payment.Service/
RUN dotnet restore payment-service/Payment.Service/Payment.Service.csproj

COPY domain/ domain/
COPY shared/ shared/
COPY payment-service/ payment-service/
RUN dotnet publish payment-service/Payment.Service/Payment.Service.csproj -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 8080
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "Payment.Service.dll"]
