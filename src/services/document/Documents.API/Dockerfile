FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["src/services/document/Documents.API/Documents.API.csproj", "src/services/document/Documents.API/"]
COPY ["src/services/document/Documents.CQRS/Documents.CQRS.csproj", "src/services/document/Documents.CQRS/"]
COPY ["src/services/document/Documents.Objects/Documents.Objects.csproj", "src/services/document/Documents.Objects/"]
COPY ["src/tools/MedEasy.Objects/MedEasy.Objects.csproj", "src/tools/MedEasy.Objects/"]
COPY ["src/tools/MedEasy.Tools/MedEasy.Tools.csproj", "src/tools/MedEasy.Tools/"]
COPY ["src/tools/MedEasy.CQRS.Core/MedEasy.CQRS.Core.csproj", "src/tools/MedEasy.CQRS.Core/"]
COPY ["src/tools/MedEasy.RestObjects/MedEasy.RestObjects.csproj", "src/tools/MedEasy.RestObjects/"]
COPY ["src/tools/MedEasy.DTO/MedEasy.DTO.csproj", "src/tools/MedEasy.DTO/"]
COPY ["src/tools/MedEasy.DAL/MedEasy.DAL.csproj", "src/tools/MedEasy.DAL/"]
COPY ["src/services/document/Documents.DTO/Documents.DTO.csproj", "src/services/document/Documents.DTO/"]
COPY ["src/tools/MedEasy.DAL.EFStore/MedEasy.DAL.EFStore.csproj", "src/tools/MedEasy.DAL.EFStore/"]
COPY ["src/services/document/Documents.DataStore.SqlServer/Documents.DataStore.SqlServer.csproj", "src/services/document/Documents.DataStore.SqlServer/"]
COPY ["src/services/MedEasy.DataStores.Core/MedEasy.DataStores.Core.csproj", "src/services/MedEasy.DataStores.Core/"]
COPY ["src/services/document/Documents.Mapping/Documents.Mapping.csproj", "src/services/document/Documents.Mapping/"]
COPY ["src/tools/MedEasy.Mapping/MedEasy.Mapping.csproj", "src/tools/MedEasy.Mapping/"]
COPY ["src/services/MedEasy.Core/MedEasy.Core.csproj", "src/services/MedEasy.Core/"]
RUN dotnet restore "src/services/document/Documents.API/Documents.API.csproj"
COPY . .
WORKDIR "/src/src/services/document/Documents.API"
RUN dotnet build "Documents.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Documents.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Documents.API.dll"]