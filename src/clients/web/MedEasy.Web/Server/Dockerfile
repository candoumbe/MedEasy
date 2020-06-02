#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["src/clients/web/MedEasy.Web/Server/MedEasy.Web.Server.csproj", "src/clients/web/MedEasy.Web/Server/"]
COPY ["src/clients/web/MedEasy.Web/Shared/MedEasy.Web.Shared.csproj", "src/clients/web/MedEasy.Web/Shared/"]
COPY ["src/clients/web/MedEasy.Web/Client/MedEasy.Web.Client.csproj", "src/clients/web/MedEasy.Web/Client/"]
COPY ["src/services/identity/Identity.Models/Identity.Models.csproj", "src/services/identity/Identity.Models/"]
COPY ["src/tools/MedEasy.RestObjects/MedEasy.RestObjects.csproj", "src/tools/MedEasy.RestObjects/"]
RUN dotnet restore "src/clients/web/MedEasy.Web/Server/MedEasy.Web.Server.csproj"
COPY . .
WORKDIR "/src/src/clients/web/MedEasy.Web/Server"
RUN dotnet build "MedEasy.Web.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MedEasy.Web.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MedEasy.Web.Server.dll"]