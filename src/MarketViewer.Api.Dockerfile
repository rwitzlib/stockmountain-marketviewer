FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine as publish
WORKDIR /src

ARG PROJECT_DIRECTORY="MarketViewer.Api"

COPY . .

RUN dotnet restore $PROJECT_DIRECTORY
RUN dotnet publish -c Release -o ./publish --no-restore $PROJECT_DIRECTORY

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app

EXPOSE 8080
EXPOSE 443

COPY --from=publish /src/publish/* .

ENTRYPOINT ["dotnet", "MarketViewer.Api.dll"]