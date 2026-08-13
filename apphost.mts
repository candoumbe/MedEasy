// Aspire TypeScript AppHost
// For more information, see: https://aspire.dev

import { createBuilder } from "./.aspire/modules/aspire.mjs";

const builder = await createBuilder();

const adminUserName = await builder.addParameter("keycloakAdminUserName", {
  secret: true,
});
const adminPassword = await builder.addParameter("keycloakAdminPassword", {
  secret: true,
});

const keycloak = await builder.addKeycloak("keycloak", {
  adminUsername: adminUserName,
  adminPassword: adminPassword,
})
.withRealmImport("./agenda/src/Agenda.AppHost/keycloak/agenda-realm.json");

const keycloakHttpEndpoint = keycloak.getEndpoint("http");

const postgres = await builder.addPostgres("postgres");

const messaging = await builder.addRabbitMQ("messaging");

const agendaImageTag = "0.2-scalar-fails-to-start-in-azurelinux-image.4e02b27";

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
};

const agendaMigrator = await builder
  .addContainer(images.agenda.worker.name, images.agenda.worker.registry)
  .withReference(postgres).waitFor(postgres)
  .withIconName("DatabaseLightningRegular");

const agendaApi = await builder
  .addContainer(images.agenda.api.name, images.agenda.api.registry)
  .withReference(postgres)
  .waitFor(postgres)
  .withReference(messaging)
  .waitFor(messaging)
  .withReference(keycloak)
  .waitFor(keycloak)
  .waitForCompletion(agendaMigrator)
  .withChildRelationship(agendaMigrator)
  .withEnvironment(
    "AGENDA_AUTH_AUTHORITY",
    `${keycloakHttpEndpoint}/realms/agenda`,
  )
  .withEnvironment("AGENDA_AUTH_CLIENT_ID", "agenda-frontend")
  .withEnvironment("AGENDA_AUTH_SCOPE", "openid profile email agenda-audience")
  .withEnvironment("SERILOG__MINIMUMLEVEL__DEFAULT", "Trace")
  .withHttpEndpoint({ name: "http", env: "PORT", targetPort: 8080 })
  .withOtlpExporter()
  .withExternalHttpEndpoints();


const agenda = await builder
  .addContainer(images.agenda.frontend.name, images.agenda.frontend.registry)
  .waitFor(agendaApi)
  .withChildRelationship(agendaApi)
  .withEnvironment("API_HTTP", `${agendaApi.getEndpoint("http")}`)
  .withReference(keycloak)
  .waitFor(keycloak)
  // Ask Aspire to allocate a port and pass it to the app via the PORT environment variable
  .withHttpEndpoint({ env: "PORT", targetPort: 3000 })
  .withExternalHttpEndpoints()
  .withEnvironment("AGENDA_AUTH_AUTHORITY", `${keycloakHttpEndpoint}/realms/agenda`)
  .withEnvironment("AGENDA_AUTH_CLIENT_ID", "agenda-frontend")
  .withEnvironment("AGENDA_AUTH_SCOPE", "openid profile email agenda-audience")
  .withBindMount("./agenda/src/Agenda.Frontend/nginx.conf", "/etc/nginx/nginx.conf", { isReadOnly: true });

await builder.build().run();
