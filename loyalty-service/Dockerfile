FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /src

COPY domain/Ecommerce.Events/Ecommerce.Events.csproj domain/Ecommerce.Events/
COPY domain/Ecommerce.Model/Ecommerce.Model.csproj domain/Ecommerce.Model/
COPY shared/Ecommerce.Shared.Infrastructure/Ecommerce.Shared.Infrastructure.csproj shared/Ecommerce.Shared.Infrastructure/
COPY shared/Ecommerce.Shared.Protos/Ecommerce.Shared.Protos.csproj shared/Ecommerce.Shared.Protos/
COPY loyalty-service/Loyalty.Application/Loyalty.Application.csproj loyalty-service/Loyalty.Application/
COPY loyalty-service/Loyalty.Infrastructure/Loyalty.Infrastructure.csproj loyalty-service/Loyalty.Infrastructure/
COPY loyalty-service/Loyalty.Service/Loyalty.Service.csproj loyalty-service/Loyalty.Service/
RUN dotnet restore loyalty-service/Loyalty.Service/Loyalty.Service.csproj

COPY domain/ domain/
COPY shared/ shared/
COPY loyalty-service/ loyalty-service/
RUN dotnet publish loyalty-service/Loyalty.Service/Loyalty.Service.csproj -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 8080
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "Loyalty.Service.dll"]
