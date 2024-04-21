FROM nginx AS base

ARG Environment

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /app

ARG CI_MARKETVIEWER_REGISTRY

COPY . .
RUN dotnet nuget add source "https://gitlab.com/api/v4/groups/66690872/-/packages/nuget/index.json" \
      --name "Gitlab Registry" --username "rwitzlib" \
      --password $CI_MARKETVIEWER_REGISTRY --store-password-in-clear-text

RUN . .
RUN dotnet restore

RUN dotnet build -c Release

# Test Stage
FROM build AS test
WORKDIR /app

RUN dotnet test --verbosity normal

# Publish Stage
FROM build AS publish
WORKDIR /app

RUN dotnet publish -c Release --no-restore --no-build -o /publish

# Final Stage
FROM base AS final
WORKDIR /usr/share/nginx/html

COPY --from=publish /publish/wwwroot .

COPY ./MarketViewer.Web/nginx.conf /etc/nginx/nginx.conf
RUN sed -i "s/replaceme/${Environment}/" /etc/nginx/nginx.conf

EXPOSE 8080
EXPOSE 80
EXPOSE 443
