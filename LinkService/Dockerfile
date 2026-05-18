FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY nuget.config .
COPY LinkService.sln .
COPY src/LinkService.Domain/LinkService.Domain.csproj src/LinkService.Domain/
COPY src/LinkService.Application/LinkService.Application.csproj src/LinkService.Application/
COPY src/LinkService.Infrastructure/LinkService.Infrastructure.csproj src/LinkService.Infrastructure/
COPY src/LinkService.Web/LinkService.Web.csproj src/LinkService.Web/
RUN dotnet restore src/LinkService.Web/LinkService.Web.csproj

COPY . .
RUN dotnet publish src/LinkService.Web/LinkService.Web.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "LinkService.Web.dll"]
