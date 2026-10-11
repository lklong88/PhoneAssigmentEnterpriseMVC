# Multi-stage .NET 8 build for Render (Docker runtime)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source
COPY src/CUSoCalPhoneAssignments.Web/CUSoCalPhoneAssignments.Web.csproj src/CUSoCalPhoneAssignments.Web/
RUN dotnet restore src/CUSoCalPhoneAssignments.Web/CUSoCalPhoneAssignments.Web.csproj
COPY src/CUSoCalPhoneAssignments.Web/ src/CUSoCalPhoneAssignments.Web/
RUN dotnet publish src/CUSoCalPhoneAssignments.Web/CUSoCalPhoneAssignments.Web.csproj -c Release -o /publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /publish .
ENV ASPNETCORE_URLS=http://0.0.0.0:10000
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DATA_DIRECTORY=/tmp/cusocal-data
EXPOSE 10000
USER $APP_UID
ENTRYPOINT ["dotnet", "CUSoCalPhoneAssignments.Web.dll"]
