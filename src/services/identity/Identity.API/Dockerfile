FROM microsoft/aspnetcore:2.0-nanoserver-1709 AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/aspnetcore-build:2.0-nanoserver-1709 AS build
WORKDIR /src
COPY src/services/identity/Identity.API/Identity.API.csproj src/services/identity/Identity.API/
COPY src/services/identity/Identity.Validators/Identity.Validators.csproj src/services/identity/Identity.Validators/
COPY src/services/identity/Identity.DTO/Identity.DTO.csproj src/services/identity/Identity.DTO/
COPY src/tools/MedEasy.DTO/MedEasy.DTO.csproj src/tools/MedEasy.DTO/
COPY src/tools/MedEasy.Data/MedEasy.Data.csproj src/tools/MedEasy.Data/
COPY src/tools/MedEasy.Tools/MedEasy.Tools.csproj src/tools/MedEasy.Tools/
COPY src/tools/MedEasy.RestObjects/MedEasy.RestObjects.csproj src/tools/MedEasy.RestObjects/
COPY src/tools/MedEasy.DAL.EFStore/MedEasy.DAL.EFStore.csproj src/tools/MedEasy.DAL.EFStore/
COPY src/tools/MedEasy.DAL/MedEasy.DAL.csproj src/tools/MedEasy.DAL/
COPY src/services/MedEasy.DataStores.Core/MedEasy.DataStores.Core.csproj src/services/MedEasy.DataStores.Core/
COPY src/tools/MedEasy.Objects/MedEasy.Objects.csproj src/tools/MedEasy.Objects/
COPY src/services/identity/Identity.Mapping/Identity.Mapping.csproj src/services/identity/Identity.Mapping/
COPY src/services/identity/Identity.Objects/Identity.Objects.csproj src/services/identity/Identity.Objects/
COPY src/Core/MedEasy.Core/MedEasy.Core.csproj src/Core/MedEasy.Core/
COPY src/tools/MedEasy.CQRS.Core/MedEasy.CQRS.Core.csproj src/tools/MedEasy.CQRS.Core/
COPY src/services/identity/Identity.DataStores.SqlServer/Identity.DataStores.SqlServer.csproj src/services/identity/Identity.DataStores.SqlServer/
COPY src/services/identity/Identity.CQRS/Identity.CQRS.csproj src/services/identity/Identity.CQRS/
COPY src/tools/MedEasy.Abstractions/MedEasy.Abstractions.csproj src/tools/MedEasy.Abstractions/
RUN dotnet restore src/services/identity/Identity.API/Identity.API.csproj
COPY . .
WORKDIR /src/src/services/identity/Identity.API
RUN dotnet build Identity.API.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish Identity.API.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Identity.API.dll"]
