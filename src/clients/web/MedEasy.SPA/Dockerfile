ARG NODE_IMAGE=node:8.11
FROM microsoft/aspnetcore:2.0 AS base
RUN apt-get update -yq \
    && apt-get install curl gnupg -yq \
    && curl -sL https://deb.nodesource.com/setup_8.x | bash \
    && apt-get install nodejs npm -yq
RUN ln -s /usr/bin/nodejs /usr/local/bin/node
RUN ln -s /usr/bin/npm /usr/local/bin/npm
WORKDIR /app
EXPOSE 80
EXPOSE 5001

FROM ${NODE_IMAGE} as node-build
WORKDIR /web
COPY src/clients/MedEasy.SPA/ .
RUN npm install
RUN npm run build


FROM microsoft/aspnetcore-build:2.0 AS build
WORKDIR /src
COPY ["src/clients/MedEasy.SPA/MedEasy.SPA.csproj", "src/clients/MedEasy.SPA/"]
RUN dotnet restore "src/clients/MedEasy.SPA/MedEasy.SPA.csproj"
COPY . .
WORKDIR "/src/src/clients/MedEasy.SPA"
RUN dotnet build "MedEasy.SPA.csproj" -c Release -o /app

FROM build as publish
WORKDIR /src/src/clients/MedEasy.SPA/wwwroot
COPY --from=node-build /web/wwwroot .
WORKDIR /src
COPY . .
RUN dotnet publish "MedEasy.SPA.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "MedEasy.SPA.dll"]