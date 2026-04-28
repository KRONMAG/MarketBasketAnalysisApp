using MarketBasketAnalysisStorage.Api.HostedServices;
using MarketBasketAnalysisStorage.Api.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

builder.WebHost.ConfigureKestrel(o => o.AllowAlternateSchemes = true);

builder.Services.Configure<DeleteAssociationRuleSetsJobSettings>(
    configuration.GetSection(nameof(DeleteAssociationRuleSetsJob)));
builder.Services.AddHostedService<DeleteAssociationRuleSetsJob>();

services.AddGrpc();
builder.Services.AddGrpcHealthChecks()
    .AddCheck("Sample", () => HealthCheckResult.Healthy());

if (builder.Environment.IsDevelopment())
    services.AddGrpcReflection();

services.AddLogging();

var app = builder.Build();

app.MapGrpcService<AssociationRuleSetStorage>();
app.MapGrpcHealthChecksService();

if (app.Environment.IsDevelopment())
    app.MapGrpcReflectionService();

await app.RunAsync();