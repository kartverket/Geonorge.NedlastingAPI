# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
# USER $APP_UID
COPY ./Geonorge.Download/Certs/_dev.geonorge.no.pem /etc/ssl/certs/
USER 150
WORKDIR /app
EXPOSE 8080
#EXPOSE 8081
ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_HTTPS_PORTS=

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Geonorge.Download/Geonorge.Download.csproj", "Geonorge.Download/"]
RUN dotnet restore "./Geonorge.Download/Geonorge.Download.csproj"
COPY . .
WORKDIR "/src/Geonorge.Download"
RUN dotnet build "./Geonorge.Download.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Geonorge.Download.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Geonorge.Download.dll"]