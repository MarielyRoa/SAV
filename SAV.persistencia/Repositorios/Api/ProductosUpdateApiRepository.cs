using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SAV.application.Repository;
using SAV.domain.Entities.Api;
using System.Text.Json;

namespace SAV.persistencia.Repositorios.Api
{
    public class ProductosUpdateApiRepository : IProductosUpdateApiRepo
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ProductosUpdateApiRepository> _logger;

        public ProductosUpdateApiRepository(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<ProductosUpdateApiRepository> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<IEnumerable<ProductosUpdate>> GetProductosUpdateAsync()
        {
            try
            {
                var apiUrl = _configuration["ExternalApi:ProductosUrl"];

                if (string.IsNullOrEmpty(apiUrl))
                {
                    _logger.LogInformation("No hay URL configurada para ProductosAPI. Usando datos mock.");
                    return GetMockProductos();
                }

                _logger.LogInformation($"Consultando API de productos: {apiUrl}");
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var productos = JsonSerializer.Deserialize<List<ProductosUpdate>>(content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    _logger.LogInformation($"Productos obtenidos de API: {productos?.Count ?? 0}");
                    return productos ?? GetMockProductos();
                }
                else
                {
                    _logger.LogWarning($"API respondió con código: {response.StatusCode}. Usando mock.");
                    return GetMockProductos();
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"Error de conexión HTTP: {ex.Message}");
                return GetMockProductos();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error inesperado: {ex.Message}");
                return GetMockProductos();
            }
        }

        private IEnumerable<ProductosUpdate> GetMockProductos()
        {
            _logger.LogInformation("Retornando productos mock");
            return new List<ProductosUpdate>
            {
                new ProductosUpdate {
                    ProductID = 2001,
                    ProductName = "Café Premium",
                    Category = "Bebidas",
                    Price = 5.99m,
                    Stock = 100
                },
                new ProductosUpdate {
                    ProductID = 2002,
                    ProductName = "Tostadas Integrales",
                    Category = "Alimentos",
                    Price = 3.50m,
                    Stock = 75
                },
                new ProductosUpdate {
                    ProductID = 2003,
                    ProductName = "Jugo de Naranja Natural",
                    Category = "Bebidas",
                    Price = 4.25m,
                    Stock = 120
                },
                new ProductosUpdate {
                    ProductID = 2004,
                    ProductName = "Yogurt Griego",
                    Category = "Lácteos",
                    Price = 2.99m,
                    Stock = 90
                },
                new ProductosUpdate {
                    ProductID = 2005,
                    ProductName = "Cereal Integral",
                    Category = "Alimentos",
                    Price = 6.50m,
                    Stock = 65
                }
            };
        }
    }
}