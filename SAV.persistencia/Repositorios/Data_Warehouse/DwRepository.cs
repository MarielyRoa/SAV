using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SAV.application.Repository;
using SAV.application.Resultado;
using SAV.domain.Entities.Api;
using SAV.domain.Entities.Data_Warehouse.Dimensions;
using SAV.domain.Entities.Data_Warehouse.Facts;
using SAV.persistencia.Repositorios.Data_Warehouse.Context;
using System.Globalization;
using System.Linq;

namespace SAV.persistencia.Repositorios.Data_Warehouse
{
    public class DwRepository : IDwRepository
    {
        private readonly DtwarehouseContext _ctx;
        private readonly IcustomersCsvRepo _csvCustomers;
        private readonly IproductsCsvRepo _csvProducts;
        private readonly IClientesUpdateApiRepo _apiClientes;
        private readonly IProductosUpdateApiRepo _apiProductos;
        private readonly IVentasHistoricasDBRepo _externalDb;
        private readonly ILogger<DwRepository> _logger;

        public DwRepository(
            DtwarehouseContext ctx,
            IcustomersCsvRepo csvCustomers,
            IproductsCsvRepo csvProducts,
            ILogger<DwRepository> logger,
            IVentasHistoricasDBRepo externalDb,
            IClientesUpdateApiRepo apiClientes,
            IProductosUpdateApiRepo apiProductos)
        {
            _ctx = ctx;
            _csvCustomers = csvCustomers;
            _csvProducts = csvProducts;
            _apiClientes = apiClientes;
            _apiProductos = apiProductos;
            _externalDb = externalDb;
            _logger = logger;
        }

        public async Task<Result> LoadDimsDataAsync()
        {
            var result = new Result();

            try
            {
                result = await CleanDimenssionTables();
                if (!result.IsSuccess) return result;

                var csvClientes = await _csvCustomers.ReadFileAsync("");
                var csvProductos = await _csvProducts.ReadFileAsync("");

                var apiClientes = _apiClientes != null
                    ? await _apiClientes.GetClientesUpdateAsync()
                    : Enumerable.Empty<ClientesUpdate>();

                var apiProductos = _apiProductos != null
                    ? await _apiProductos.GetProductosUpdateAsync()
                    : Enumerable.Empty<ProductosUpdate>();

                var ventasExternas = await _externalDb.GetVentasHistoricasAsync();

                DateTime fechaCarga = DateTime.Now;

               
                var fuentes = new List<DimFuente>
                {
                    new DimFuente { IdFuente = 1, TipoFuente = "CSV", Descripcion = "Archivo CSV de Clientes", FechaCarga = fechaCarga },
                    new DimFuente { IdFuente = 2, TipoFuente = "CSV", Descripcion = "Archivo CSV de Productos", FechaCarga = fechaCarga },
                    new DimFuente { IdFuente = 3, TipoFuente = "API", Descripcion = "API REST Clientes", FechaCarga = fechaCarga },
                    new DimFuente { IdFuente = 4, TipoFuente = "API", Descripcion = "API REST Productos", FechaCarga = fechaCarga },
                    new DimFuente { IdFuente = 5, TipoFuente = "BD Externa", Descripcion = "Base de datos ventas históricas", FechaCarga = fechaCarga }
                };

                await _ctx.DimFuentes.AddRangeAsync(fuentes);
                await _ctx.SaveChangesAsync();

                // Obtener las surrogate keys (FuenteKey) generadas por la BD
                var fuenteCsvClientesKey = fuentes.Single(f => f.IdFuente == 1).FuenteKey;
                var fuenteCsvProductosKey = fuentes.Single(f => f.IdFuente == 2).FuenteKey;
                var fuenteApiClientesKey = fuentes.Single(f => f.IdFuente == 3).FuenteKey;
                var fuenteApiProductosKey = fuentes.Single(f => f.IdFuente == 4).FuenteKey;
                var fuenteExternosKey = fuentes.Single(f => f.IdFuente == 5).FuenteKey;

                
               
                var dimClientes = new List<DimCliente>();

                dimClientes.AddRange(csvClientes.Select(c => new DimCliente
                {
                    CustomerID = c.CustomerID,  
                    FirstName = c.FirstName ?? string.Empty,
                    LastName = c.LastName ?? string.Empty,
                    Email = c.Email ?? string.Empty,
                    Phone = c.Phone ?? string.Empty,
                    City = c.City ?? string.Empty,
                    Country = c.Country ?? string.Empty,
                    Segmento = DeterminarSegmento(c.City, c.Country),
                    FechaCarga = fechaCarga,
                    IdFuente = fuenteCsvClientesKey  
                }));

                dimClientes.AddRange(apiClientes.Select(c => new DimCliente
                {
                    CustomerID = c.CustomerID,  
                    FirstName = c.FirstName ?? string.Empty,
                    LastName = c.LastName ?? string.Empty,
                    Email = c.Email ?? string.Empty,
                    Phone = c.Phone ?? string.Empty,
                    City = c.City ?? string.Empty,
                    Country = c.Country ?? string.Empty,
                    Segmento = DeterminarSegmento(c.City, c.Country),
                    FechaCarga = fechaCarga,
                    IdFuente = fuenteApiClientesKey  
                }));

                var clientesIdsExistentes = dimClientes
                    .Select(c => c.CustomerID)
                    .ToHashSet();

               
                var clientesExternos = ventasExternas
                    .Select(v => v.CustomerID)
                    .Distinct()
                    .Where(id => !clientesIdsExistentes.Contains(id))
                    .Select(id => new DimCliente
                    {
                        CustomerID = id,  
                        FirstName = $"Cliente-{id}",
                        LastName = "Histórico",
                        Email = $"cliente{id}@historico.com",
                        Phone = "N/A",
                        City = "Desconocida",
                        Country = "Desconocido",
                        Segmento = "No Clasificado",
                        FechaCarga = fechaCarga,
                        IdFuente = fuenteExternosKey  
                    })
                    .ToArray();

                dimClientes.AddRange(clientesExternos);

                await _ctx.DimClientes.AddRangeAsync(dimClientes);
                await _ctx.SaveChangesAsync();

              
                var dimProductos = new List<DimProducto>();

                dimProductos.AddRange(csvProductos.Select(p => new DimProducto
                {
                    ProductID = p.ProductID,
                    ProductName = p.ProductName ?? string.Empty,
                    Category = p.Category ?? string.Empty,
                    Price = p.Price,
                    Stock = p.Stock,
                    Marca = DeterminarMarca(p.ProductName),
                    FechaCarga = fechaCarga,
                    IdFuente = fuenteCsvProductosKey  
                }));

                dimProductos.AddRange(apiProductos.Select(p => new DimProducto
                {
                    ProductID = p.ProductID,
                    ProductName = p.ProductName ?? string.Empty,
                    Category = p.Category ?? string.Empty,
                    Price = p.Price,
                    Stock = p.Stock,
                    Marca = DeterminarMarca(p.ProductName),
                    FechaCarga = fechaCarga,
                    IdFuente = fuenteApiProductosKey  
                }));

                var productosIdsExistentes = dimProductos.Select(p => p.ProductID).ToHashSet();
                var productosExternos = ventasExternas
                    .Select(v => v.ProductID)
                    .Distinct()
                    .Where(id => !productosIdsExistentes.Contains(id))
                    .Select(id => new DimProducto
                    {
                        ProductID = id,
                        ProductName = $"Producto-{id}",
                        Category = "Sin Categoría",
                        Price = 0,
                        Stock = 0,
                        Marca = "Desconocida",
                        FechaCarga = fechaCarga,
                        IdFuente = fuenteExternosKey  
                    });

                dimProductos.AddRange(productosExternos);

                await _ctx.DimProductos.AddRangeAsync(dimProductos);
                await _ctx.SaveChangesAsync();

                // -----------------------
                // Cargar DimTiempo
                // -----------------------
                var datafecha = ventasExternas
                    .Select(fe => fe.OrderDate.Date)
                    .Distinct()
                    .Select(fe => new DimTiempo
                    {
                        Fecha = fe,
                        Año = fe.Year,
                        Mes = fe.Month,
                        nombre_mes = fe.ToString("MMMM", new CultureInfo("es-ES")),
                        trimestre = (fe.Month - 1) / 3 + 1,
                        semestre = fe.Month <= 6 ? 1 : 2,
                        semana_año = System.Globalization.ISOWeek.GetWeekOfYear(fe),
                        dia_mes = fe.Day,
                        dia_semana = fe.ToString("dddd", new CultureInfo("es-ES")),
                        es_fin_semana = (fe.DayOfWeek == DayOfWeek.Saturday || fe.DayOfWeek == DayOfWeek.Sunday),
                        es_feriado = EsFeriado(fe),
                        mes_año = $"{fe.Month:00}/{fe.Year}"
                    }).ToArray();

                await _ctx.DimTiempos.AddRangeAsync(datafecha);
                await _ctx.SaveChangesAsync();

                result.IsSuccess = true;
                result.Message = "La carga de dimensiones fue completada exitosamente.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en LoadDimsDataAsync: {Message} - Inner: {Inner}", ex.Message, ex.InnerException?.Message);
                result.IsSuccess = false;
                result.Message = ex.Message;
            }

            return result;
        }

        private async Task<Result> CleanDimenssionTables()
        {
            var result = new Result();

            try
            {
                // 1. Borrar tabla de hechos primero
                await _ctx.Database.ExecuteSqlRawAsync("DELETE FROM Fact.FactVentas");

                // 2. Borrar dimensiones (orden recomendado para evitar FK conflicts)
                await _ctx.Database.ExecuteSqlRawAsync("DELETE FROM Dimension.DimTiempo");
                await _ctx.Database.ExecuteSqlRawAsync("DELETE FROM Dimension.DimCliente");
                await _ctx.Database.ExecuteSqlRawAsync("DELETE FROM Dimension.DimProducto");
                await _ctx.Database.ExecuteSqlRawAsync("DELETE FROM Dimension.DimFuente");

                result.IsSuccess = true;
                result.Message = "Las tablas de dimensiones fueron limpiadas correctamente.";
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = $"Error limpiando dimensiones: {ex.Message}";
            }

            return result;
        }

        public async Task<int> InsertDimClienteAsync(DimCliente cliente)
        {
            await _ctx.DimClientes.AddAsync(cliente);
            return await _ctx.SaveChangesAsync();
        }

        public async Task<int> InsertDimFuenteAsync(DimFuente fuente)
        {
            await _ctx.DimFuentes.AddAsync(fuente);
            return await _ctx.SaveChangesAsync();
        }

        public async Task<int> InsertDimProductoAsync(DimProducto producto)
        {
            await _ctx.DimProductos.AddAsync(producto);
            return await _ctx.SaveChangesAsync();
        }

        public async Task<int> InsertDimTiempoAsync(DimTiempo tiempo)
        {
            await _ctx.DimTiempos.AddAsync(tiempo);
            return await _ctx.SaveChangesAsync();
        }

        public async Task<int> InsertFactVentaAsync(FactVentas fact)
        {
            await _ctx.FactVentas.AddAsync(fact);
            return await _ctx.SaveChangesAsync();
        }

        private string DeterminarSegmento(string city, string country)
        {
            if (string.IsNullOrWhiteSpace(city) || string.IsNullOrWhiteSpace(country))
                return "No Clasificado";

            var countryLower = country.ToLower();

            if (countryLower.Contains("usa") || countryLower.Contains("united states") ||
                countryLower.Contains("canada") || countryLower.Contains("mexico"))
                return "Norteamerica";

            if (countryLower.Contains("uk") || countryLower.Contains("united kingdom") ||
                countryLower.Contains("germany") || countryLower.Contains("france") ||
                countryLower.Contains("spain") || countryLower.Contains("italy"))
                return "Europa";

            if (countryLower.Contains("china") || countryLower.Contains("japan") ||
                countryLower.Contains("korea") || countryLower.Contains("singapore"))
                return "Asia Pacifico";

            if (countryLower.Contains("brasil") || countryLower.Contains("argentina") ||
                countryLower.Contains("chile") || countryLower.Contains("colombia") ||
                countryLower.Contains("dominican"))
                return "America Latina";

            return "Otros Mercados";
        }

        private string DeterminarMarca(string productName)
        {
            if (string.IsNullOrWhiteSpace(productName))
                return "Sin Marca";

            var palabras = productName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (palabras.Length == 0)
                return "Generica";

            return palabras.Length > 1 ? palabras[0] : "Generica";
        }

        private bool EsFeriado(DateTime fecha)
        {
            var feriados = new List<(int mes, int dia)>
            {
                (1, 1),   // Año Nuevo
                (1, 21),  // Dia de la Altagracia
                (2, 27),  // Dia de la Independencia
                (5, 1),   // Dia del Trabajo
                (8, 16),  // Dia de la Restauracion
                (9, 24),  // Dia de las Mercedes
                (11, 6),  // Dia de la Constitucion
                (12, 25)  // Navidad
            };

            return feriados.Any(f => f.mes == fecha.Month && f.dia == fecha.Day);
        }

        public async Task<Result> LoadFactsDataAsync()
        {
            var result = new Result();

            try
            {
                var ventasExternas = await _externalDb.GetVentasHistoricasAsync();

                var clientesDW = await _ctx.DimClientes.AsNoTracking().ToListAsync();
                var productosDW = await _ctx.DimProductos.AsNoTracking().ToListAsync();
                var tiemposDW = await _ctx.DimTiempos.AsNoTracking().ToListAsync();
                var fuenteDW = await _ctx.DimFuentes.FirstOrDefaultAsync(f => f.IdFuente == 5); // Fuente BD Externa (business id 5)

                await _ctx.FactVentas.ExecuteDeleteAsync();

                var factVentas = new List<FactVentas>();

                foreach (var venta in ventasExternas)
                {
                    
                    var cliente = clientesDW.FirstOrDefault(c => c.CustomerID == venta.CustomerID);
                    var producto = productosDW.FirstOrDefault(p => p.ProductID == venta.ProductID);
                    var tiempo = tiemposDW.FirstOrDefault(t => t.Fecha.Date == venta.OrderDate.Date);

                    if (cliente != null && producto != null && tiempo != null && fuenteDW != null)
                    {
                        var fact = new FactVentas
                        {
                            TiempoKey = tiempo.TiempoKey,
                            ProductKey = producto.ProductKey,
                            CustomerKey = cliente.CustomerKey,
                            FuenteKey = fuenteDW.FuenteKey, 
                            cantidad = venta.Quantity,
                            precio_unitario = venta.UnitPrice,
                            total_venta = venta.TotalPrice,
                            fecha_carga = DateTime.Now,
                            OrderID = venta.OrderID.ToString(),
                            Status = venta.Status
                        };
                        factVentas.Add(fact);
                    }
                }

                await _ctx.FactVentas.AddRangeAsync(factVentas);
                await _ctx.SaveChangesAsync();

                result.IsSuccess = true;
                result.Message = $"Se cargaron {factVentas.Count} registros en FactVentas.";
                _logger.LogInformation(result.Message);
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = $"Error cargando hechos: {ex.Message}";
                _logger.LogError(ex, "Error en LoadFactsDataAsync");
            }

            return result;
        }

    }
}