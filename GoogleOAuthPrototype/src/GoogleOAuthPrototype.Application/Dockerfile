FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/GoogleOAuthPrototype.Application/GoogleOAuthPrototype.Application.csproj", "src/GoogleOAuthPrototype.Application/"]
RUN dotnet restore "src/GoogleOAuthPrototype.Application/GoogleOAuthPrototype.Application.csproj"
COPY . .
WORKDIR "/src/src/GoogleOAuthPrototype.Application"
RUN dotnet build "GoogleOAuthPrototype.Application.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "GoogleOAuthPrototype.Application.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GoogleOAuthPrototype.Application.dll"]
