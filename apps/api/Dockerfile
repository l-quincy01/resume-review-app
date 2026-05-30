FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

COPY resume-tracker.sln ./
COPY apps/api/api.csproj apps/api/
COPY apps/api.Tests/api.Tests.csproj apps/api.Tests/

RUN dotnet restore apps/api/api.csproj

COPY apps/api apps/api

RUN dotnet publish apps/api/api.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

WORKDIR /app

ENV ASPNETCORE_URLS=http://0.0.0.0:5053
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish ./

EXPOSE 5053

ENTRYPOINT ["dotnet", "api.dll"]
