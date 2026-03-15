using DevGuardian.AgentRuntime;
using DevGuardian.Tools;
using Microsoft.SemanticKernel;

var builder = WebApplication.CreateBuilder(args);

// ── Configuration ──────────────────────────────────────────────────────────
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

// ── Services ───────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title   = "DevGuardian AI API",
        Version = "v1",
        Description = "Spec-driven multi-agent AI system for automated incident detection, " +
                      "root-cause analysis, fix generation, and deployment planning."
    });
});

// ── Semantic Kernel ────────────────────────────────────────────────────────
builder.Services.AddSingleton<Kernel>(sp =>
{
    var cfg        = sp.GetRequiredService<IConfiguration>();
    var endpoint   = cfg["AzureOpenAI:Endpoint"]
                     ?? throw new InvalidOperationException("AzureOpenAI:Endpoint is required.");
    var key        = cfg["AzureOpenAI:Key"]
                     ?? throw new InvalidOperationException("AzureOpenAI:Key is required.");
    var deployment = cfg["AzureOpenAI:Deployment"] ?? "gpt-4o";

    var kernel = KernelFactory.Create(endpoint, key, deployment);

    // Register tool plugins
    var githubToken = cfg["GitHub:Token"] ?? string.Empty;
    var githubOwner = cfg["GitHub:Owner"] ?? "your-org";
    var githubRepo  = cfg["GitHub:Repo"]  ?? "your-repo";

    PluginRegistrar.RegisterAll(kernel, githubToken, githubOwner, githubRepo);

    return kernel;
});

// ── Agent Runtime ──────────────────────────────────────────────────────────
builder.Services.AddSingleton<SpecLoader>(sp =>
{
    var cfg      = sp.GetRequiredService<IConfiguration>();
    var specsDir = cfg["DevGuardian:SpecsPath"]
                   // fall back to specs/ next to the binary
                   ?? Path.Combine(AppContext.BaseDirectory, "specs");
    return new SpecLoader(specsDir);
});

builder.Services.AddSingleton<DevGuardian.AgentRuntime.AgentRuntime>();
builder.Services.AddSingleton<WorkflowEngine>();

// ── CORS (allow all for hackathon demo) ────────────────────────────────────
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

// ── App ────────────────────────────────────────────────────────────────────
var app = builder.Build();

app.UseCors();
app.UseDefaultFiles();   // serves index.html at /
app.UseStaticFiles();    // serves wwwroot/

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "DevGuardian AI v1");
        c.RoutePrefix = string.Empty; // serve Swagger at root
    });
}

app.UseAuthorization();
app.MapControllers();

app.Run();
