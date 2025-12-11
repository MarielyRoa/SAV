using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SAV.application.Repository;
using SAV.application.Resultado;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SAV.workerLoad
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public Worker(
            ILogger<Worker> logger,
            IServiceProvider serviceProvider,
            IConfiguration configuration)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker ETL iniciado. Esperando 10 segundos para inicializacion...");

           
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

            
            if (_configuration.GetValue<bool>("WorkerSettings:RunAtStartup", true))
            {
                await RunETLProcessAsync();
            }

           
            var intervalMinutes = _configuration.GetValue<int>("WorkerSettings:IntervalMinutes", 60);

            _logger.LogInformation($"Worker configurado para ejecutar cada {intervalMinutes} minutos.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await RunETLProcessAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en la ejecucion del ETL");
                }

               
                _logger.LogInformation($"Esperando {intervalMinutes} minutos para proxima ejecucion...");
                await Task.Delay(TimeSpan.FromMinutes(intervalMinutes), stoppingToken);
            }

            _logger.LogInformation("Worker detenido.");
        }

        private async Task RunETLProcessAsync()
        {
            
            if (!await _semaphore.WaitAsync(0))
            {
                _logger.LogWarning("ETL ya en ejecucion. Saltando esta iteracion.");
                return;
            }

            var startTime = DateTime.Now;

            try
            {
                _logger.LogInformation($"Iniciando proceso ETL completo a las {startTime:HH:mm:ss}");

                using (var scope = _serviceProvider.CreateScope())
                {
                    var dwRepo = scope.ServiceProvider.GetRequiredService<IDwRepository>();

                    // FASE 1: CARGAR DIMENSIONES
                    _logger.LogInformation("FASE 1: Cargando dimensiones...");

                    var dimResult = await dwRepo.LoadDimsDataAsync();

                    if (dimResult.IsSuccess)
                    {
                        _logger.LogInformation($"Dimensiones cargadas exitosamente: {dimResult.Message}");

                        // FASE 2: CARGAR HECHOS
                        _logger.LogInformation("FASE 2: Cargando hechos (FactVentas)...");

                        // Verificar si el metodo existe en el repositorio
                        var methodInfo = dwRepo.GetType().GetMethod("LoadFactsDataAsync");
                        if (methodInfo != null)
                        {
                            var task = (Task<Result>)methodInfo.Invoke(dwRepo, null);
                            var factResult = await task;

                            if (factResult.IsSuccess)
                            {
                                _logger.LogInformation($"Hechos cargados: {factResult.Message}");
                            }
                            else
                            {
                                _logger.LogError($"Error cargando hechos: {factResult.Message}");
                            }
                        }
                        else
                        {
                            _logger.LogWarning("Metodo LoadFactsDataAsync no implementado aun.");
                            _logger.LogInformation("Implementa LoadFactsDataAsync en DwRepository.cs para cargar FactVentas");
                        }
                    }
                    else
                    {
                        _logger.LogError($"Error en fase de dimensiones: {dimResult.Message}");
                    }

                    // FASE 3: VERIFICAR DATOS
                    _logger.LogInformation("FASE 3: Verificando datos cargados...");
                    await VerifyDataLoaded(scope);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error critico en el proceso ETL");
            }
            finally
            {
                _semaphore.Release();

                var endTime = DateTime.Now;
                var duration = endTime - startTime;

                _logger.LogInformation($"Proceso ETL completado en {duration.TotalMinutes:F2} minutos");
                _logger.LogInformation($"Proxima ejecucion en {_configuration.GetValue<int>("WorkerSettings:IntervalMinutes", 60)} minutos");
            }
        }

        private async Task VerifyDataLoaded(IServiceScope scope)
        {
            try
            {
                var dwContext = scope.ServiceProvider.GetRequiredService<persistencia.Repositorios.Data_Warehouse.Context.DtwarehouseContext>();

                var clientesCount = await dwContext.DimClientes.CountAsync();
                var productosCount = await dwContext.DimProductos.CountAsync();
                var fuentesCount = await dwContext.DimFuentes.CountAsync();
                var tiemposCount = await dwContext.DimTiempos.CountAsync();
                var ventasCount = await dwContext.FactVentas.CountAsync();

                _logger.LogInformation($"RESUMEN DATOS CARGADOS:");
                _logger.LogInformation($"   - DimClientes: {clientesCount} registros");
                _logger.LogInformation($"   - DimProductos: {productosCount} registros");
                _logger.LogInformation($"   - DimFuentes: {fuentesCount} registros");
                _logger.LogInformation($"   - DimTiempos: {tiemposCount} registros");
                _logger.LogInformation($"   - FactVentas: {ventasCount} registros");

                if (clientesCount == 0)
                    _logger.LogWarning("No se cargaron clientes en las dimensiones");

                if (ventasCount == 0)
                    _logger.LogWarning("No se cargaron ventas en los hechos");
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"No se pudo verificar datos: {ex.Message}");
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deteniendo Worker ETL...");
            await base.StopAsync(cancellationToken);
        }
    }
}