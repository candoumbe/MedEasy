FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY ["src/clients/web/MedEasy.Web/MedEasy.Web.csproj", "src/clients/web/MedEasy.Web/"]
COPY ["src/services/identity/Identity.Models/Identity.Models.csproj", "src/services/identity/Identity.Models/"]
RUN dotnet restore "src/clients/web/MedEasy.Web/MedEasy.Web.csproj"
COPY . .
WORKDIR "/src/src/clients/web/MedEasy.Web"
RUN dotnet build "MedEasy.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MedEasy.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MedEasy.Web.dll"]