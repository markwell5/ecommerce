FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /src

COPY domain/Ecommerce.Events/Ecommerce.Events.csproj domain/Ecommerce.Events/
COPY domain/Ecommerce.Model/Ecommerce.Model.csproj domain/Ecommerce.Model/
COPY shared/Ecommerce.Shared.Infrastructure/Ecommerce.Shared.Infrastructure.csproj shared/Ecommerce.Shared.Infrastructure/
COPY user-service/User.Application/User.Application.csproj user-service/User.Application/
COPY user-service/User.Infrastructure/User.Infrastructure.csproj user-service/User.Infrastructure/
COPY user-service/User.Service/User.Service.csproj user-service/User.Service/
RUN dotnet restore user-service/User.Service/User.Service.csproj

COPY domain/ domain/
COPY shared/ shared/
COPY user-service/ user-service/
RUN dotnet publish user-service/User.Service/User.Service.csproj -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 8080
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "User.Service.dll"]
