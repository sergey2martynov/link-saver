FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY nuget.config .
COPY UserService.sln .
COPY src/UserService.Domain/UserService.Domain.csproj src/UserService.Domain/
COPY src/UserService.Application/UserService.Application.csproj src/UserService.Application/
COPY src/UserService.Infrastructure/UserService.Infrastructure.csproj src/UserService.Infrastructure/
COPY src/UserService.Web/UserService.Web.csproj src/UserService.Web/
RUN dotnet restore src/UserService.Web/UserService.Web.csproj

COPY . .
RUN dotnet publish src/UserService.Web/UserService.Web.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "UserService.Web.dll"]
