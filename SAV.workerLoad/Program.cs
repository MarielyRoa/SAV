using Microsoft.EntityFrameworkCore;
using SAV.application.Repository;
using SAV.persistencia.Repositorios.Api;
using SAV.persistencia.Repositorios.Csv;
using SAV.persistencia.Repositorios.Data_Warehouse;
using SAV.persistencia.Repositorios.Data_Warehouse.Context;
using SAV.persistencia.Repositorios.Db_externa;
using SAV.persistencia.Repositorios.Db_externa.Context;


namespace SAV.workerLoad
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();
            builder.Logging.SetMinimumLevel(LogLevel.Information);

            // ========== CONFIGURACIÓN DE BASES DE DATOS ==========

            // 1. Data Warehouse Context (destino final)
            builder.Services.AddDbContext<DtwarehouseContext>(options =>
            {
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DtwarehouseConnString"),
                    sqlOptions =>
                    {
                        sqlOptions.CommandTimeout(300); 
                        sqlOptions.EnableRetryOnFailure(3);
                    });
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });

            // 2. Base de datos externa (ventas históricas - origen)
            builder.Services.AddDbContext<ventasHistoricasContext>(options =>
            {
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DbExterna"),
                    sqlOptions => sqlOptions.CommandTimeout(300));
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });

            // ========== CONFIGURACIÓN DE HTTP CLIENT ==========

            _ = builder.Services.AddHttpClient<IClientesUpdateApiRepo, ClientesUpdateApiRepository>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("User-Agent", "ETL-Worker");
            });

            builder.Services.AddHttpClient<IProductosUpdateApiRepo, ProductosUpdateApiRepository>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("User-Agent", "ETL-Worker");
            });

            // HttpClient genérico para otros usos
            builder.Services.AddHttpClient();

            // Repositorios CSV
            builder.Services.AddScoped<IcustomersCsvRepo, CustomersReaderRepository>();
            builder.Services.AddScoped<IproductsCsvRepo, ProductsReaderRepository>();
            builder.Services.AddScoped<IordersCsvRepo, OrdersReaderRepository>();
            builder.Services.AddScoped<Iorder_detailsCsvRepo, Order_detailsReaderRepository>();

         

            // Repositorio de Ventas Históricas (BD externa)
            builder.Services.AddScoped<IVentasHistoricasDBRepo, VentasHistoricasDbRepository>();

            // Repositorio del Data Warehouse
            builder.Services.AddScoped<IDwRepository, DwRepository>();

            // Servicio de procesamiento ETL
            builder.Services.AddScoped<application.Interfaces.IDataWarehouseService, application.Services.DataWarehouseService>();

            // ========== CONFIGURACIÓN DEL WORKER ==========

            builder.Services.AddHostedService<Worker>();

            builder.Services.Configure<HostOptions>(options =>
            {
                options.ServicesStartConcurrently = false;
                options.ServicesStopConcurrently = false;
                options.ShutdownTimeout = TimeSpan.FromMinutes(5);
            });

            var host = builder.Build();

            
            if (builder.Environment.IsDevelopment())
            {
                using var scope = host.Services.CreateScope();
                var dwContext = scope.ServiceProvider.GetRequiredService<DtwarehouseContext>();
                var externalContext = scope.ServiceProvider.GetRequiredService<ventasHistoricasContext>();

                try
                {
                    await dwContext.Database.EnsureCreatedAsync();
                    await externalContext.Database.EnsureCreatedAsync();
                    Console.WriteLine("Bases de datos verificadas/creadas.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creando bases de datos: {ex.Message}");
                }
            }

            host.Run();
        }
    }
}