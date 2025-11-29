using KorID.MigrationService;
using KorID.Data;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddHostedService<Worker>();

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddSource(Worker.ActivitySourceName));

builder.Services.AddDataServices(builder.Configuration, "koriddb");

var host = builder.Build();
host.Run();
