FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /code
COPY Marker.ServiceDefaults/ Marker.ServiceDefaults/
COPY Marker.WebApi/ Marker.WebApi/
WORKDIR /code/Marker.WebApi
RUN dotnet restore
RUN dotnet build -c Release

FROM build AS publish
RUN dotnet publish -c Release

FROM base AS final
COPY --from=publish /code/Marker.WebApi/bin/Release/net8.0/publish/ .
ENTRYPOINT ["dotnet", "Marker.WebApi.dll"]