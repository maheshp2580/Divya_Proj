using Hangfire;
using Serilog;
using SmartJobRunner.Application;
using SmartJobRunner.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Layer Dependencies
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Default SQLite DB setup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SmartJobRunner.Infrastructure.Persistence.ApplicationDbContext>();
    context.Database.EnsureCreated(); // Creates DB schema based on entities
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseDefaultFiles(); // Set default files like index.html
app.UseStaticFiles(); // Allow serving of static files from wwwroot

app.UseAuthorization();

// Hangfire Dashboard mapping
app.UseHangfireDashboard();

app.MapControllers();

app.Run();
