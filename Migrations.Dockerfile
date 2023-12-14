FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
ENV PATH="${PATH}:/root/.dotnet/tools"
RUN dotnet tool install --global dotnet-ef
WORKDIR /app
COPY . .
RUN dotnet ef migrations bundle --project src/Asiago.Data/Asiago.Data.csproj --output out/efbundle

FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /migrations
# $APP_UID defaults to 1654 (user 'app') from environment in base container
USER $APP_UID
COPY --from=build-env /app/out .
CMD ["./efbundle"]
