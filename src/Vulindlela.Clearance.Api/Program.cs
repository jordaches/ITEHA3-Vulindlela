using Microsoft.EntityFrameworkCore;
using Vulindlela.Clearance.Api.Business;
using Vulindlela.Clearance.Api.Business.Interfaces;
using Vulindlela.Clearance.Api.Business.Queue;
using Vulindlela.Clearance.Api.Business.Workers;
using Vulindlela.Clearance.Api.Data;
using Vulindlela.Clearance.Api.Data.Repositories;
using Vulindlela.Clearance.Api.Data.Seed;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://localhost:9003");

builder.Services.AddDbContext<ClearanceDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("ClearanceDb")));

// Data
builder.Services.AddScoped<IDocumentRuleRepository, DocumentRuleRepository>();
builder.Services.AddScoped<IClearanceStatusRepository, ClearanceStatusRepository>();
builder.Services.AddScoped<ISubmissionRepository, SubmissionRepository>();
builder.Services.AddScoped<IUnitOfWork, ClearanceUnitOfWork>();

// Business
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddScoped<IDocumentRuleEvaluator, DocumentRuleEvaluator>();
builder.Services.AddScoped<IVerificationService, VerificationService>();
builder.Services.AddScoped<ISubmissionService, SubmissionService>();

// Queue + worker
builder.Services.AddSingleton<IDocumentQueue, ChannelDocumentQueue>();
builder.Services.AddHostedService<ClearanceWorker>();

builder.Services.AddControllers();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ClearanceDbContext>();
    db.Database.EnsureCreated();
    ClearanceSeeder.Seed(db);
}

app.MapControllers();
app.Run();

public partial class Program { }