FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-env
WORKDIR /app
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
# $APP_UID defaults to 1654 (user 'app') from environment in base container
USER $APP_UID
COPY --from=build-env /app/out .
CMD ["./Asiago"]
