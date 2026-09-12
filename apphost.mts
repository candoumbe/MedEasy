// Aspire TypeScript AppHost
// For more information, see: https://aspire.dev

import { createBuilder, refExpr } from "./.aspire/modules/aspire.mjs";

const builder = await createBuilder();

const adminUserName = await builder.addParameter("keycloakAdminUserName", {
  secret: false,
});
const adminPassword = await builder.addParameter("keycloakAdminPassword", {
  secret: true,
});

const keycloak = await builder.addKeycloak("keycloak", {
  adminUsername: adminUserName,
  adminPassword: adminPassword,
})
.withRealmImport("./agenda/src/Agenda.AppHost/keycloak/agenda-realm.json");

const keycloakHttpEndpoint = await keycloak.getEndpoint("http");

const messaging = await builder.addRabbitMQ("messaging");

const agendaImageTag = "0.3.0-alpha";
const documentsImageTag = "0.1-alpha";

const images = {
  agenda: {
    // The agenda worker is responsible for initializing the database and running migrations.
    worker: {
      name: "agenda-init-db",
      registry: `ghcr.io/candoumbe/agenda.worker:${agendaImageTag}`,
    },
    // The agenda API is the main application that serves the agenda functionality.
    api: {
      name: "agenda-api",
      registry: `ghcr.io/candoumbe/agenda.api:${agendaImageTag}`,
    },
    // The agenda frontend is the web application that provides the user interface for the agenda functionality.
    frontend: {
      name: "agenda-frontend",
      registry: `ghcr.io/candoumbe/agenda.frontend:${agendaImageTag}`,
    },
  },
  documents: {
    // The documents worker is responsible for initializing the database and running migrations.
    worker: {
      name: "documents-init-db",
      registry: `ghcr.io/candoumbe/documents.worker:${documentsImageTag}`,
    },
    // The documents API is the main application that serves the documents functionality.
    api: {
      name: "documents-api",
      registry: `ghcr.io/candoumbe/documents.api:${documentsImageTag}`,
    },

    storage: {
      name: "documents-storage",
      registry: `docker.io/minio/minio:RELEASE.2025-09-07T16-13-09Z`,
    },
  },
  physiotrack: {
    api: {
      name: "physiotrack-api",
      registry: `ghcr.io/candoumbe/physiotrack-api:0.1-alpha`,
    }
  }
};

const agendaDb = await builder.addPostgres("agenda-db");

const agendaMigrator = await builder
  .addContainer(images.agenda.worker.name, images.agenda.worker.registry)
  .withReference(agendaDb, {connectionName: "postgres"}).waitFor(agendaDb)
  .withIconName("DatabaseLightningRegular");

const keycloakAgendaRealm = refExpr `${keycloakHttpEndpoint}/realms/agenda`;
const agendaApi = await builder
  .addContainer(images.agenda.api.name, images.agenda.api.registry)
  .withReference(agendaDb, {connectionName: "postgres"}).waitFor(agendaDb)
  .withReference(messaging).waitFor(messaging)
  .withReference(keycloak).waitFor(keycloak)
  .waitForCompletion(agendaMigrator).withChildRelationship(agendaMigrator)
  .withEnvironment("AGENDA_AUTH_AUTHORITY",keycloakAgendaRealm)
  .withEnvironment("AGENDA_AUTH_CLIENT_ID", "agenda-frontend")
  .withEnvironment("AGENDA_AUTH_SCOPE", "openid profile email agenda-audience")
  .withEnvironment("SERILOG__MINIMUMLEVEL__DEFAULT", "Verbose")
  .withEnvironment("ASPNETCORE_HTTP_PORTS", "8080")
  .withEnvironment("ASPNETCORE_URLS", "http://+:8080")
  .withHttpEndpoint({ name: "http", env: "PORT", targetPort: 8080 })
  .withOtlpExporter()
  .withExternalHttpEndpoints();


const agendaApiHttpEndpoint = refExpr `${await agendaApi.getEndpoint("http")}`;
const agenda = await builder
  .addContainer(images.agenda.frontend.name, images.agenda.frontend.registry)
  .waitFor(agendaApi).withChildRelationship(agendaApi)
  .withEnvironment("API_HTTP", agendaApiHttpEndpoint)
  .withReference(keycloak).waitFor(keycloak)
  // Ask Aspire to allocate a port and pass it to the app via the PORT environment variable
  .withHttpEndpoint({ env: "PORT", targetPort: 8080 })
  .withExternalHttpEndpoints()
  .withEnvironment("AGENDA_AUTH_AUTHORITY", keycloakAgendaRealm)
  .withEnvironment("AGENDA_AUTH_CLIENT_ID", "agenda-frontend")
  .withEnvironment("AGENDA_AUTH_SCOPE", "openid profile email agenda-audience")
  .withBindMount("./agenda/src/Agenda.Frontend/nginx.conf", "/etc/nginx/nginx.conf", { isReadOnly: true });

const documentsDb = await builder.addPostgres("documents-db");
const documentsMigrator = await builder
  .addContainer(images.documents.worker.name, images.documents.worker.registry)
  .withReference(documentsDb, {connectionName: "postgres"}).waitFor(documentsDb)
  .withIconName("DatabaseLightningRegular");

const documentsStorage = await builder.addMinioContainer("documents-storage")
// Ask Aspire to allocate a port and pass it to the app via the PORT environment variable
  .withExternalHttpEndpoints()

const documentsApi = await builder
  .addContainer(images.documents.api.name, images.documents.api.registry)
  .withReference(documentsStorage, {connectionName: "minio"}).waitFor(documentsStorage)
  .withReference(documentsDb, {connectionName: "postgres"}).waitFor(documentsDb)
  .withReference(messaging).waitFor(messaging)
  .waitForCompletion(documentsMigrator).withChildRelationship(documentsMigrator)
  .withReference(keycloak).waitFor(keycloak)
  .withEnvironment("DOCUMENTS_AUTH_AUTHORITY", `${keycloakHttpEndpoint}/realms/documents`)
  .withEnvironment("DOCUMENTS_AUTH_CLIENT_ID", "documents-frontend")
  .withEnvironment("DOCUMENTS_AUTH_SCOPE", "openid profile email documents-audience")
  .withEnvironment("SERILOG__MINIMUMLEVEL__DEFAULT", "Verbose")
  .withEnvironment("SERILOG__WriteTo__0__Name", "Console")
  .withEnvironment("SERILOG__WriteTo__0__Args__OutputTemplate", "{Timestamp:HH:mm:ss.fff zzz} [{Level:u3}] {Message}{NewLine}{Exception}")
  .withEnvironment("ASPNETCORE_HTTP_PORTS", "8181")
  .withEnvironment("ASPNETCORE_URLS", "http://+:8181")
  .withHttpEndpoint({ name: "http", env: "PORT", targetPort: 8181 })
  .withOtlpExporter()
  .withExternalHttpEndpoints();
  
const physiotrackApi = await builder
  .addContainer(images.physiotrack.api.name, images.physiotrack.api.registry)
  .withHttpEndpoint({ name: "http", env: "PORT", targetPort: 8000 })
  .withExternalHttpEndpoints()
  .withIconName("Python")
  .withOtlpExporter();
  

await builder.build().run();
