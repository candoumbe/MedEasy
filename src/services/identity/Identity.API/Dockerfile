#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["src/services/identity/Identity.API/Identity.API.csproj", "src/services/identity/Identity.API/"]
COPY ["src/services/identity/Identity.Validators/Identity.Validators.csproj", "src/services/identity/Identity.Validators/"]
COPY ["src/tools/MedEasy.Abstractions/MedEasy.Abstractions.csproj", "src/tools/MedEasy.Abstractions/"]
COPY ["src/services/identity/Identity.DTO/Identity.DTO.csproj", "src/services/identity/Identity.DTO/"]
COPY ["src/tools/MedEasy.DTO/MedEasy.DTO.csproj", "src/tools/MedEasy.DTO/"]
COPY ["src/tools/MedEasy.RestObjects/MedEasy.RestObjects.csproj", "src/tools/MedEasy.RestObjects/"]
COPY ["src/tools/MedEasy.DAL/MedEasy.DAL.csproj", "src/tools/MedEasy.DAL/"]
COPY ["src/services/identity/Identity.Objects/Identity.Objects.csproj", "src/services/identity/Identity.Objects/"]
COPY ["src/tools/MedEasy.Objects/MedEasy.Objects.csproj", "src/tools/MedEasy.Objects/"]
COPY ["src/tools/MedEasy.Tools/MedEasy.Tools.csproj", "src/tools/MedEasy.Tools/"]
COPY ["src/tools/MedEasy.DAL.EFStore/MedEasy.DAL.EFStore.csproj", "src/tools/MedEasy.DAL.EFStore/"]
COPY ["src/services/MedEasy.DataStores.Core/MedEasy.DataStores.Core.csproj", "src/services/MedEasy.DataStores.Core/"]
COPY ["src/services/identity/Identity.Mapping/Identity.Mapping.csproj", "src/services/identity/Identity.Mapping/"]
COPY ["src/tools/MedEasy.Mapping/MedEasy.Mapping.csproj", "src/tools/MedEasy.Mapping/"]
COPY ["src/services/MedEasy.Core/MedEasy.Core.csproj", "src/services/MedEasy.Core/"]
COPY ["src/tools/MedEasy.CQRS.Core/MedEasy.CQRS.Core.csproj", "src/tools/MedEasy.CQRS.Core/"]
COPY ["src/services/identity/Identity.CQRS.Handlers.EFCore/Identity.CQRS.Handlers.EFCore.csproj", "src/services/identity/Identity.CQRS.Handlers.EFCore/"]
COPY ["src/services/identity/Identity.CQRS/Identity.CQRS.csproj", "src/services/identity/Identity.CQRS/"]
COPY ["src/services/identity/Identity.DataStores.SqlServer/Identity.DataStores.SqlServer.csproj", "src/services/identity/Identity.DataStores.SqlServer/"]
RUN dotnet restore "src/services/identity/Identity.API/Identity.API.csproj"
COPY . .
WORKDIR "/src/src/services/identity/Identity.API"
RUN dotnet build "Identity.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Identity.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Identity.API.dll"]