using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var postgresUsername = builder.AddParameter("postgres-user", value: "postgres");
var postgresPassword = builder.AddParameter("postgres-password", value: "postgres", secret: true);


var postgres = builder.AddPostgres("postgres")
    .WithUserName(postgresUsername)
    .WithPassword(postgresPassword)
    .WithDataVolume()
    .WithHostPort(5432)
    .WithPgAdmin(pgAdmin => pgAdmin.WithHostPort(5050));
var postgresdb = postgres.AddDatabase("koriddb");

var migrations = builder.AddProject<Projects.KorID_MigrationService>("migrations")
    .WithReference(postgresdb)
    .WaitFor(postgresdb);

var koridapi = builder.AddProject<Projects.KorID_API>("koridapi")
    .WithReference(postgresdb)
    .WithReference(migrations)
    .WaitForCompletion(migrations)
    .WithExternalHttpEndpoints();

builder.AddNpmApp("korid-client", "../korid.client")
    .WithReference(koridapi)
    .WaitFor(koridapi)
    .WithEnvironment("BROWSER", "none")
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

builder.Build().Run();
