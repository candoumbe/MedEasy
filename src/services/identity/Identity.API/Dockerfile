#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
ENV TZ Europe/Paris
RUN cp /usr/share/zoneinfo/Europe/Paris /etc/localtime

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["core.props", "."]
COPY ["tests.props", "."]
COPY ["core-api.props", "."]
COPY ["global.json", "."]
COPY [".editorconfig", "."]

COPY ["src/services/identity/Identity.API/Identity.API.csproj", "src/services/identity/Identity.API/"]
COPY ["src/services/identity/Identity.Validators/Identity.Validators.csproj", "src/services/identity/Identity.Validators/"]
COPY ["src/tools/MedEasy.Abstractions/MedEasy.Abstractions.csproj", "src/tools/MedEasy.Abstractions/"]
COPY ["src/services/identity/Identity.DTO/Identity.DTO.csproj", "src/services/identity/Identity.DTO/"]
COPY ["src/tools/MedEasy.Ids/MedEasy.Ids.csproj", "src/tools/MedEasy.Ids/"]
COPY ["src/tools/MedEasy.DTO/MedEasy.DTO.csproj", "src/tools/MedEasy.DTO/"]
COPY ["src/tools/MedEasy.RestObjects/MedEasy.RestObjects.csproj", "src/tools/MedEasy.RestObjects/"]
COPY ["src/services/identity/Identity.Ids/Identity.Ids.csproj", "src/services/identity/Identity.Ids/"]
COPY ["src/tools/MedEasy.DAL/MedEasy.DAL.csproj", "src/tools/MedEasy.DAL/"]
COPY ["src/services/identity/Identity.Objects/Identity.Objects.csproj", "src/services/identity/Identity.Objects/"]
COPY ["src/tools/MedEasy.Objects/MedEasy.Objects.csproj", "src/tools/MedEasy.Objects/"]
COPY ["src/tools/MedEasy.Tools/MedEasy.Tools.csproj", "src/tools/MedEasy.Tools/"]
COPY ["src/tools/MedEasy.DAL.EFStore/MedEasy.DAL.EFStore.csproj", "src/tools/MedEasy.DAL.EFStore/"]
COPY ["src/services/identity/Identity.DataStores/Identity.DataStores.csproj", "src/services/identity/Identity.DataStores/"]
COPY ["src/services/MedEasy.DataStores.Core/MedEasy.DataStores.Core.csproj", "src/services/MedEasy.DataStores.Core/"]
COPY ["src/services/MedEasy.AspnetCore.AsyncInitializers/MedEasy.AspnetCore.AsyncInitializers.csproj", "src/services/MedEasy.AspnetCore.AsyncInitializers/"]
COPY ["src/services/identity/Identity.DataStores.Sqlite/Identity.DataStores.Sqlite.csproj", "src/services/identity/Identity.DataStores.Sqlite/"]
COPY ["src/services/identity/Identity.Mapping/Identity.Mapping.csproj", "src/services/identity/Identity.Mapping/"]
COPY ["src/tools/MedEasy.Mapping/MedEasy.Mapping.csproj", "src/tools/MedEasy.Mapping/"]
COPY ["src/services/MedEasy.Core/MedEasy.Core.csproj", "src/services/MedEasy.Core/"]
COPY ["src/tools/MedEasy.CQRS.Core/MedEasy.CQRS.Core.csproj", "src/tools/MedEasy.CQRS.Core/"]
COPY ["src/services/MedEasy.Attributes/MedEasy.Attributes.csproj", "src/services/MedEasy.Attributes/"]
COPY ["src/services/identity/Identity.DataStores.Postgres/Identity.DataStores.Postgres.csproj", "src/services/identity/Identity.DataStores.Postgres/"]
COPY ["src/services/identity/Identity.CQRS.Handlers.EFCore/Identity.CQRS.Handlers.EFCore.csproj", "src/services/identity/Identity.CQRS.Handlers.EFCore/"]
COPY ["src/services/identity/Identity.CQRS/Identity.CQRS.csproj", "src/services/identity/Identity.CQRS/"]

WORKDIR "/src/src/services/identity/Identity.API"
RUN dotnet restore "Identity.API.csproj" --source "https://api.nuget.org/v3/index.json"
RUN dotnet build "Identity.API.csproj" -c Release -o /app/build --no-restore

FROM build AS publish
RUN dotnet publish "Identity.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Identity.API.dll"]