using SAV.api.Data.Context;
using SAV.application.Repository;
using Microsoft.EntityFrameworkCore; 
using SAV.persistencia.Repositorios.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Configurar Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "SAV API - Sistema de Ventas",
        Version = "v1",
        Description = "API para consultar clientes y productos actualizados"
    });
});


builder.Services.AddDbContext<ApiContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("ApiConnection")
        ?? "Server=DESKTOP-B3FNPSK\\SQLEXPRESS;Database=ApiDB;Trusted_Connection=True;TrustServerCertificate=True;";

    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.CommandTimeout(60);
        sqlOptions.EnableRetryOnFailure(3);
    });
});

builder.Services.AddHttpClient<IClientesUpdateApiRepo, ClientesUpdateApiRepository>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("User-Agent", "SAV-API");
});

builder.Services.AddHttpClient<IProductosUpdateApiRepo, ProductosUpdateApiRepository>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("User-Agent", "SAV-API");
});


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});


builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Information);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "SAV API v1");
    options.RoutePrefix = string.Empty; // Swagger en la raíz
});

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Lifetime.ApplicationStarted.Register(() =>
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("========================================");
    logger.LogInformation("?? SAV API iniciada exitosamente");
    logger.LogInformation("?? Swagger UI: http://localhost:3000/");
    logger.LogInformation("?? Endpoints disponibles:");
    logger.LogInformation("   GET /api/ClientesApi/GetClientes");
    logger.LogInformation("   GET /api/ProductosApi/GetProductos");
    logger.LogInformation("========================================");
});

app.Run();
