FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY nuget.config .
COPY Gateway.sln .
COPY src/Gateway.Web/Gateway.Web.csproj src/Gateway.Web/
RUN dotnet restore src/Gateway.Web/Gateway.Web.csproj

COPY . .
RUN dotnet publish src/Gateway.Web/Gateway.Web.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Gateway.Web.dll"]
