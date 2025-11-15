var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin(pgAdmin => pgAdmin.WithHostPort(5050));
var postgresdb = postgres.AddDatabase("koriddb");

var koridapi = builder.AddProject<Projects.KorID_API>("koridapi")
    .WithReference(postgresdb)
    .WithExternalHttpEndpoints();

builder.AddNpmApp("korid-client", "../korid.client")
    .WithReference(koridapi)
    .WaitFor(koridapi)
    .WithEnvironment("BROWSER", "none")
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

builder.Build().Run();
