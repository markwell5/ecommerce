FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /src

COPY domain/Ecommerce.Events/Ecommerce.Events.csproj domain/Ecommerce.Events/
COPY domain/Ecommerce.Model/Ecommerce.Model.csproj domain/Ecommerce.Model/
COPY shared/Ecommerce.Shared.Infrastructure/Ecommerce.Shared.Infrastructure.csproj shared/Ecommerce.Shared.Infrastructure/
COPY shared/Ecommerce.Shared.Protos/Ecommerce.Shared.Protos.csproj shared/Ecommerce.Shared.Protos/
COPY audit-service/Audit.Application/Audit.Application.csproj audit-service/Audit.Application/
COPY audit-service/Audit.Infrastructure/Audit.Infrastructure.csproj audit-service/Audit.Infrastructure/
COPY audit-service/Audit.Service/Audit.Service.csproj audit-service/Audit.Service/
RUN dotnet restore audit-service/Audit.Service/Audit.Service.csproj

COPY domain/ domain/
COPY shared/ shared/
COPY audit-service/ audit-service/
RUN dotnet publish audit-service/Audit.Service/Audit.Service.csproj -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 8080
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "Audit.Service.dll"]
