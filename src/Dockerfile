#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["NbaStats.Injury.Web.Api/NbaStats.Injury.Web.Api.csproj", "NbaStats.Injury.Web.Api/"]
RUN dotnet restore "NbaStats.Injury.Web.Api/NbaStats.Injury.Web.Api.csproj"
COPY . .
WORKDIR "/src/NbaStats.Injury.Web.Api"
RUN dotnet build "NbaStats.Injury.Web.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NbaStats.Injury.Web.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NbaStats.Injury.Web.Api.dll"]