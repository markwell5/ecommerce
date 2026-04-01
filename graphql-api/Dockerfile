FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /src

COPY shared/Ecommerce.Shared.Infrastructure/Ecommerce.Shared.Infrastructure.csproj shared/Ecommerce.Shared.Infrastructure/
COPY shared/Ecommerce.Shared.Protos/Ecommerce.Shared.Protos.csproj shared/Ecommerce.Shared.Protos/
COPY shared/Ecommerce.Shared.GrpcClients/Ecommerce.Shared.GrpcClients.csproj shared/Ecommerce.Shared.GrpcClients/
COPY domain/Ecommerce.Events/Ecommerce.Events.csproj domain/Ecommerce.Events/
COPY graphql-api/GraphQL.Api/GraphQL.Api.csproj graphql-api/GraphQL.Api/
RUN dotnet restore graphql-api/GraphQL.Api/GraphQL.Api.csproj

COPY domain/ domain/
COPY shared/ shared/
COPY graphql-api/ graphql-api/
RUN dotnet publish graphql-api/GraphQL.Api/GraphQL.Api.csproj -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 8080
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "GraphQL.Api.dll"]
