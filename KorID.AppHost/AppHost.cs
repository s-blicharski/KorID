var builder = DistributedApplication.CreateBuilder(args);

var koridapi = builder.AddProject<Projects.KorID_API>("koridapi")
    .WithExternalHttpEndpoints();

builder.AddNpmApp("korid-client", "../korid.client")
    .WithReference(koridapi)
    .WaitFor(koridapi)
    .WithEnvironment("BROWSER", "none")
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

builder.Build().Run();
