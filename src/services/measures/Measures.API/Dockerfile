FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
ENV TZ Europe/Paris
RUN cp /usr/share/zoneinfo/Europe/Paris /etc/localtime

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["nuget.config", "."]
COPY . .

WORKDIR "/src/src/services/measures/Measures.API"
RUN dotnet restore "Measures.API.csproj"
RUN dotnet build "Measures.API.csproj" -c Release -o /app/build --no-restore

FROM build AS publish
RUN dotnet publish "Measures.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Measures.API.dll"]