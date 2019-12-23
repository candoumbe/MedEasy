FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch AS build
WORKDIR /src
COPY ["src/services/Patients/Patients.API/Patients.API.csproj", "src/services/Patients/Patients.API/"]
COPY ["src/tools/MedEasy.Abstractions/MedEasy.Abstractions.csproj", "src/tools/MedEasy.Abstractions/"]
COPY ["src/tools/MedEasy.DAL.EFStore/MedEasy.DAL.EFStore.csproj", "src/tools/MedEasy.DAL.EFStore/"]
COPY ["src/tools/MedEasy.DAL/MedEasy.DAL.csproj", "src/tools/MedEasy.DAL/"]
COPY ["src/tools/MedEasy.Data/MedEasy.Data.csproj", "src/tools/MedEasy.Data/"]
COPY ["src/tools/MedEasy.Tools/MedEasy.Tools.csproj", "src/tools/MedEasy.Tools/"]
COPY ["src/services/Patients/Patients.DTO/Patients.DTO.csproj", "src/services/Patients/Patients.DTO/"]
COPY ["src/tools/MedEasy.RestObjects/MedEasy.RestObjects.csproj", "src/tools/MedEasy.RestObjects/"]
COPY ["src/tools/MedEasy.DTO/MedEasy.DTO.csproj", "src/tools/MedEasy.DTO/"]
COPY ["src/services/Patients/Patients.Mapping/Patients.Mapping.csproj", "src/services/Patients/Patients.Mapping/"]
COPY ["src/tools/MedEasy.Mapping/MedEasy.Mapping.csproj", "src/tools/MedEasy.Mapping/"]
COPY ["src/tools/MedEasy.Objects/MedEasy.Objects.csproj", "src/tools/MedEasy.Objects/"]
COPY ["src/services/Patients/Patients.Objects/Patients.Objects.csproj", "src/services/Patients/Patients.Objects/"]
COPY ["src/services/patients/Patients.Context/Patients.Context.csproj", "src/services/patients/Patients.Context/"]
COPY ["src/services/MedEasy.DataStores.Core/MedEasy.DataStores.Core.csproj", "src/services/MedEasy.DataStores.Core/"]
COPY ["src/services/Patients/Patients.Validators/Patients.Validators.csproj", "src/services/Patients/Patients.Validators/"]
COPY ["src/tools/MedEasy.Validators/MedEasy.Validators.csproj", "src/tools/MedEasy.Validators/"]
COPY ["src/Core/MedEasy.Core/MedEasy.Core.csproj", "src/Core/MedEasy.Core/"]
COPY ["src/tools/MedEasy.CQRS.Core/MedEasy.CQRS.Core.csproj", "src/tools/MedEasy.CQRS.Core/"]
RUN dotnet restore "src/services/Patients/Patients.API/Patients.API.csproj"
COPY . .
WORKDIR "/src/src/services/Patients/Patients.API"

RUN dotnet build "Patients.API.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Patients.API.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Patients.API.dll"]