// Aspire TypeScript AppHost
// For more information, see: https://aspire.dev

import { createBuilder } from './.aspire/modules/aspire.mjs';

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
});

const keycloakHttpEndpoint = keycloak.getEndpoint("http");

const postgres = await builder.addPostgres("postgres");

const messaging = await builder.addRabbitMQ("messaging");

const agendaMigrations = await builder.addContainer("agenda-migrations", "ghcr.io/candoumbe/agenda.worker:0.2-alpha")
  .withReference(postgres).waitFor(postgres)
  .withIconName("DatabaseLightningRegular")

const agenda = await builder.addContainer("agenda-api", "ghcr.io/candoumbe/agenda.api:0.2-alpha")
    .withReference(postgres).waitFor(postgres)
    .withReference(messaging).waitFor(messaging)
    .withReference(keycloak).waitFor(keycloak)
    .waitForCompletion(agendaMigrations).withChildRelationship(agendaMigrations)
    .withEnvironment("AGENDA_AUTH_AUTHORITY", `${keycloakHttpEndpoint}/realms/agenda`)
    .withEnvironment("AGENDA_AUTH_CLIENT_ID", "agenda-frontend")
    .withEnvironment("AGENDA_AUTH_SCOPE", "openid profile email agenda-audience")
    .withHttpEndpoint({ env: 'PORT' , targetPort: 8080})
    .withExternalHttpEndpoints()

// Add your resources here, for example:
// const redis = await builder.addContainer("cache", "redis:latest");
// const postgres = await builder.addPostgres("db");

await builder.build().run();