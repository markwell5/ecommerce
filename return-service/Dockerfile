FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /src

COPY domain/Ecommerce.Events/Ecommerce.Events.csproj domain/Ecommerce.Events/
COPY domain/Ecommerce.Model/Ecommerce.Model.csproj domain/Ecommerce.Model/
COPY shared/Ecommerce.Shared.Infrastructure/Ecommerce.Shared.Infrastructure.csproj shared/Ecommerce.Shared.Infrastructure/
COPY shared/Ecommerce.Shared.Protos/Ecommerce.Shared.Protos.csproj shared/Ecommerce.Shared.Protos/
COPY return-service/Return.Application/Return.Application.csproj return-service/Return.Application/
COPY return-service/Return.Infrastructure/Return.Infrastructure.csproj return-service/Return.Infrastructure/
COPY return-service/Return.Service/Return.Service.csproj return-service/Return.Service/
RUN dotnet restore return-service/Return.Service/Return.Service.csproj

COPY domain/ domain/
COPY shared/ shared/
COPY return-service/ return-service/
RUN dotnet publish return-service/Return.Service/Return.Service.csproj -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 8080
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "Return.Service.dll"]
