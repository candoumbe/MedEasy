#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["src/services/Patients/Patients.API/Patients.API.csproj", "src/services/Patients/Patients.API/"]
COPY ["src/tools/MedEasy.Abstractions/MedEasy.Abstractions.csproj", "src/tools/MedEasy.Abstractions/"]
COPY ["src/tools/MedEasy.DAL.EFStore/MedEasy.DAL.EFStore.csproj", "src/tools/MedEasy.DAL.EFStore/"]
COPY ["src/tools/MedEasy.DAL/MedEasy.DAL.csproj", "src/tools/MedEasy.DAL/"]
COPY ["src/services/Patients/Patients.DTO/Patients.DTO.csproj", "src/services/Patients/Patients.DTO/"]
COPY ["src/tools/MedEasy.RestObjects/MedEasy.RestObjects.csproj", "src/tools/MedEasy.RestObjects/"]
COPY ["src/tools/MedEasy.DTO/MedEasy.DTO.csproj", "src/tools/MedEasy.DTO/"]
COPY ["src/services/patients/Patients.Ids/Patients.Ids.csproj", "src/services/patients/Patients.Ids/"]
COPY ["src/tools/MedEasy.Ids/MedEasy.Ids.csproj", "src/tools/MedEasy.Ids/"]
COPY ["src/services/Patients/Patients.Context/Patients.DataStores.csproj", "src/services/Patients/Patients.Context/"]
COPY ["src/services/MedEasy.DataStores.Core/MedEasy.DataStores.Core.csproj", "src/services/MedEasy.DataStores.Core/"]
COPY ["src/services/MedEasy.AspnetCore.AsyncInitializers/MedEasy.AspnetCore.AsyncInitializers.csproj", "src/services/MedEasy.AspnetCore.AsyncInitializers/"]
COPY ["src/tools/MedEasy.Objects/MedEasy.Objects.csproj", "src/tools/MedEasy.Objects/"]
COPY ["src/tools/MedEasy.Tools/MedEasy.Tools.csproj", "src/tools/MedEasy.Tools/"]
COPY ["src/services/Patients/Patients.Objects/Patients.Objects.csproj", "src/services/Patients/Patients.Objects/"]
COPY ["src/services/patients/Patients.DataStores.Postgres/Patients.DataStores.Postgres.csproj", "src/services/patients/Patients.DataStores.Postgres/"]
COPY ["src/services/Patients/Patients.CQRS/Patients.CQRS.csproj", "src/services/Patients/Patients.CQRS/"]
COPY ["src/tools/MedEasy.CQRS.Core/MedEasy.CQRS.Core.csproj", "src/tools/MedEasy.CQRS.Core/"]
COPY ["src/services/patients/Patients.Events/Patients.Events.csproj", "src/services/patients/Patients.Events/"]
COPY ["src/services/patients/Patients.DataStores.Sqlite/Patients.DataStores.Sqlite.csproj", "src/services/patients/Patients.DataStores.Sqlite/"]
COPY ["src/services/Patients/Patients.Mapping/Patients.Mapping.csproj", "src/services/Patients/Patients.Mapping/"]
COPY ["src/tools/MedEasy.Mapping/MedEasy.Mapping.csproj", "src/tools/MedEasy.Mapping/"]
COPY ["src/services/MedEasy.Core/MedEasy.Core.csproj", "src/services/MedEasy.Core/"]
COPY ["src/services/MedEasy.Attributes/MedEasy.Attributes.csproj", "src/services/MedEasy.Attributes/"]
COPY ["src/services/Patients/Patients.Validators/Patients.Validators.csproj", "src/services/Patients/Patients.Validators/"]
COPY ["src/tools/MedEasy.Validators/MedEasy.Validators.csproj", "src/tools/MedEasy.Validators/"]
RUN dotnet restore "src/services/Patients/Patients.API/Patients.API.csproj"
COPY . .
WORKDIR "/src/src/services/Patients/Patients.API"
RUN dotnet build "Patients.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Patients.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Patients.API.dll"]