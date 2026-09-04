FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY CargoAPI.sln ./
COPY CargoAPI.API/CargoAPI.API.csproj CargoAPI.API/
COPY CargoAPI.Business/CargoAPI.Business.csproj CargoAPI.Business/
COPY CargoAPI.DataAccess/CargoAPI.DataAccess.csproj CargoAPI.DataAccess/
COPY CargoAPI.Entities/CargoAPI.Entities.csproj CargoAPI.Entities/
COPY CargoAPI.Tests/CargoAPI.Tests.csproj CargoAPI.Tests/
RUN dotnet restore CargoAPI.API/CargoAPI.API.csproj

COPY . .
RUN dotnet publish CargoAPI.API/CargoAPI.API.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "CargoAPI.API.dll"]
