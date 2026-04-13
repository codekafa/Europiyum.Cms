FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG PROJECT_PATH
ARG BUILD_CONFIG=Release
WORKDIR /src

COPY . .
RUN dotnet restore "$PROJECT_PATH"
RUN dotnet publish "$PROJECT_PATH" -c "$BUILD_CONFIG" -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
ARG PROJECT_DLL
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true
ENV PROJECT_DLL=${PROJECT_DLL}

COPY --from=build /app/publish ./

EXPOSE 8080
ENTRYPOINT ["sh", "-c", "dotnet ${PROJECT_DLL}"]
