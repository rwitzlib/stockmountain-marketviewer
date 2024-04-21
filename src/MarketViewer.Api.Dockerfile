FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine as publish
WORKDIR /src

ARG PRIVATE_NUGET_REGISTRY
ARG PRIVATE_NUGET_REGISTRY_USER
ARG PRIVATE_NUGET_REGISTRY_PASS

ARG PROJECT_DIRECTORY="MarketViewer.Api"

COPY . .

RUN dotnet nuget add source $PRIVATE_NUGET_REGISTRY \
      --username $PRIVATE_NUGET_REGISTRY_USER --password $PRIVATE_NUGET_REGISTRY_PASS \
      --store-password-in-clear-text

RUN dotnet restore $PROJECT_DIRECTORY
RUN dotnet publish -c Release -o ./publish --no-restore $PROJECT_DIRECTORY

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app

EXPOSE 8080
EXPOSE 443

COPY --from=publish /src/publish/* .

ENTRYPOINT ["dotnet", "MarketViewer.Api.dll"]