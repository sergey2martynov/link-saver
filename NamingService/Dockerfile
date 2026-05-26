FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY nuget.config .
COPY NamingService.sln .
COPY src/NamingService.Application/NamingService.Application.csproj src/NamingService.Application/
COPY src/NamingService.Infrastructure/NamingService.Infrastructure.csproj src/NamingService.Infrastructure/
COPY src/NamingService.Web/NamingService.Web.csproj src/NamingService.Web/
RUN dotnet restore src/NamingService.Web/NamingService.Web.csproj

COPY . .
RUN dotnet publish src/NamingService.Web/NamingService.Web.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "NamingService.Web.dll"]
