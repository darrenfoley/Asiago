FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-env
RUN dotnet tool install --global dotnet-ef
ENV PATH="${PATH}:/root/.dotnet/tools"
WORKDIR /app
COPY . .
RUN dotnet restore src/Asiago.Data/Asiago.Data.csproj
RUN dotnet build src/Asiago.Data/Asiago.Data.csproj --configuration Release
RUN dotnet ef migrations bundle --project src/Asiago.Data/Asiago.Data.csproj --configuration Release --no-build --output out/efbundle

FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /migrations
# $APP_UID defaults to 1654 (user 'app') from environment in base container
USER $APP_UID
COPY --from=build-env /app/out .
CMD ["./efbundle"]
