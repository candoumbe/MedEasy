#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["src/services/agenda/Agenda.API/Agenda.API.csproj", "src/services/agenda/Agenda.API/"]
COPY ["src/services/agenda/Agenda.Mapping/Agenda.Mapping.csproj", "src/services/agenda/Agenda.Mapping/"]
COPY ["src/services/agenda/Agenda.DTO/Agenda.DTO.csproj", "src/services/agenda/Agenda.DTO/"]
COPY ["src/tools/MedEasy.RestObjects/MedEasy.RestObjects.csproj", "src/tools/MedEasy.RestObjects/"]
COPY ["src/tools/MedEasy.DTO/MedEasy.DTO.csproj", "src/tools/MedEasy.DTO/"]
COPY ["src/services/agenda/Agenda.Objects/Agenda.Objects.csproj", "src/services/agenda/Agenda.Objects/"]
COPY ["src/tools/MedEasy.Objects/MedEasy.Objects.csproj", "src/tools/MedEasy.Objects/"]
COPY ["src/tools/MedEasy.Tools/MedEasy.Tools.csproj", "src/tools/MedEasy.Tools/"]
COPY ["src/tools/MedEasy.Mapping/MedEasy.Mapping.csproj", "src/tools/MedEasy.Mapping/"]
COPY ["src/services/agenda/Agenda.Models/Agenda.Models.csproj", "src/services/agenda/Agenda.Models/"]
COPY ["src/services/MedEasy.Models/MedEasy.Models.csproj", "src/services/MedEasy.Models/"]
COPY ["src/services/MedEasy.Attributes/MedEasy.Attributes.csproj", "src/services/MedEasy.Attributes/"]
COPY ["src/tools/MedEasy.DAL.EFStore/MedEasy.DAL.EFStore.csproj", "src/tools/MedEasy.DAL.EFStore/"]
COPY ["src/tools/MedEasy.DAL/MedEasy.DAL.csproj", "src/tools/MedEasy.DAL/"]
COPY ["src/tools/MedEasy.CQRS.Core/MedEasy.CQRS.Core.csproj", "src/tools/MedEasy.CQRS.Core/"]
COPY ["src/services/agenda/Agenda.Validators/Agenda.Validators.csproj", "src/services/agenda/Agenda.Validators/"]
COPY ["src/tools/MedEasy.Abstractions/MedEasy.Abstractions.csproj", "src/tools/MedEasy.Abstractions/"]
COPY ["src/tools/MedEasy.Validators/MedEasy.Validators.csproj", "src/tools/MedEasy.Validators/"]
COPY ["src/services/MedEasy.Core/MedEasy.Core.csproj", "src/services/MedEasy.Core/"]
COPY ["src/services/agenda/Agenda.DataStores/Agenda.DataStores.csproj", "src/services/agenda/Agenda.DataStores/"]
COPY ["src/services/MedEasy.DataStores.Core/MedEasy.DataStores.Core.csproj", "src/services/MedEasy.DataStores.Core/"]
COPY ["src/services/agenda/Agenda.CQRS/Agenda.CQRS.csproj", "src/services/agenda/Agenda.CQRS/"]
RUN dotnet restore "src/services/agenda/Agenda.API/Agenda.API.csproj"
COPY . .
WORKDIR "/src/src/services/agenda/Agenda.API"
RUN dotnet build "Agenda.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Agenda.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Agenda.API.dll"]