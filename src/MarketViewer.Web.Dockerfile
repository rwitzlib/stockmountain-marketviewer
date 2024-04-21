FROM nginx AS base

ARG Environment

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS publish
WORKDIR /app

ARG PRIVATE_NUGET_REGISTRY
ARG PRIVATE_NUGET_REGISTRY_USER
ARG PRIVATE_NUGET_REGISTRY_PASS

ARG PROJECT_DIRECTORY="MarketViewer.Web"

COPY . .

RUN dotnet nuget add source $PRIVATE_NUGET_REGISTRY \
      --username $PRIVATE_NUGET_REGISTRY_USER --password $PRIVATE_NUGET_REGISTRY_PASS \
      --store-password-in-clear-text

RUN dotnet restore $PROJECT_DIRECTORY
RUN dotnet publish -c Release -o ./publish --no-restore $PROJECT_DIRECTORY

FROM base AS final
WORKDIR /usr/share/nginx/html

ARG PROJECT_DIRECTORY="MarketViewer.Web"

COPY --from=publish /app/publish/wwwroot .

COPY ./$PROJECT_DIRECTORY/nginx.conf /etc/nginx/nginx.conf
RUN sed -i "s/replaceme/${Environment}/" /etc/nginx/nginx.conf

EXPOSE 8080
EXPOSE 443
