FROM microsoft/aspnetcore:2.0-nanoserver-1709 AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/aspnetcore-build:2.0-nanoserver-1709 AS build
WORKDIR /src
COPY src/services/Measures/Measures.API/Measures.API.csproj src/services/Measures/Measures.API/
COPY src/tools/MedEasy.DAL.EFStore/MedEasy.DAL.EFStore.csproj src/tools/MedEasy.DAL.EFStore/
COPY src/tools/MedEasy.DAL/MedEasy.DAL.csproj src/tools/MedEasy.DAL/
COPY src/tools/MedEasy.Data/MedEasy.Data.csproj src/tools/MedEasy.Data/
COPY src/tools/MedEasy.Tools/MedEasy.Tools.csproj src/tools/MedEasy.Tools/
COPY src/services/Measures/Measures.CQRS/Measures.CQRS.csproj src/services/Measures/Measures.CQRS/
COPY src/tools/MedEasy.CQRS.Core/MedEasy.CQRS.Core.csproj src/tools/MedEasy.CQRS.Core/
COPY src/tools/MedEasy.Objects/MedEasy.Objects.csproj src/tools/MedEasy.Objects/
COPY src/tools/MedEasy.RestObjects/MedEasy.RestObjects.csproj src/tools/MedEasy.RestObjects/
COPY src/tools/MedEasy.DTO/MedEasy.DTO.csproj src/tools/MedEasy.DTO/
COPY src/services/Measures/Measures.DTO/Measures.DTO.csproj src/services/Measures/Measures.DTO/
COPY src/services/Measures/Measures.Objects/Measures.Objects.csproj src/services/Measures/Measures.Objects/
COPY src/tools/MedEasy.Validators/MedEasy.Validators.csproj src/tools/MedEasy.Validators/
COPY src/services/Measures/Measures.Validators/Measures.Validators.csproj src/services/Measures/Measures.Validators/
COPY src/Core/MedEasy.Core/MedEasy.Core.csproj src/Core/MedEasy.Core/
COPY src/services/identity/Identity.DTO/Identity.DTO.csproj src/services/identity/Identity.DTO/
COPY src/services/Measures/Measures.Mapping/Measures.Mapping.csproj src/services/Measures/Measures.Mapping/
COPY src/services/Measures/Measures.Context/Measures.Context.csproj src/services/Measures/Measures.Context/
RUN dotnet restore src/services/Measures/Measures.API/Measures.API.csproj
COPY . .
WORKDIR /src/src/services/Measures/Measures.API
RUN dotnet build Measures.API.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish Measures.API.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Measures.API.dll"]
