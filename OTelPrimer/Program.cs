using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

using OTelPrimer.Configuration;
using OTelPrimer.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

builder.Services.Configure<PongConfig>(builder.Configuration.GetSection("PongConfig"));
builder.Services.Configure<PingConfig>(builder.Configuration.GetSection("PingConfig"));

builder.Services.AddSingleton<PingSender>();
builder.Services.AddSingleton<IPingSender, ActivityEnrichedPingSenderDecorator>();
builder.Services.AddHostedService<PingerService>();

var otelCollector = builder.Configuration.GetValue<string?>("OtelCollector");
var serviceName = builder.Configuration.GetValue<string?>("ServiceName") ?? "Unknown";

if (otelCollector is not null)
{
    var otelUrl = new Uri(otelCollector);
    AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

    builder.Services.AddOpenTelemetry().WithTracing(
        telemetryBuilder =>
        {
            telemetryBuilder.AddHttpClientInstrumentation();
            telemetryBuilder.AddAspNetCoreInstrumentation();
            telemetryBuilder.AddHttpClientInstrumentation();
            telemetryBuilder.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName));
            
            telemetryBuilder.AddOtlpExporter(options =>
            {
                options.Endpoint = otelUrl;
                options.Protocol = OtlpExportProtocol.Grpc;
            });

            if (builder.Environment.IsDevelopment())
            {
                telemetryBuilder.AddConsoleExporter();
            }
        }).StartWithHost();

    // Configure logging
    builder.Logging.AddOpenTelemetry(telemetryBuilder =>
    {
        telemetryBuilder.IncludeFormattedMessage = true;
        telemetryBuilder.IncludeScopes = true;
        telemetryBuilder.ParseStateValues = true;
        telemetryBuilder.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName));
        telemetryBuilder.AddOtlpExporter(options => options.Endpoint = otelUrl);
    });
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();