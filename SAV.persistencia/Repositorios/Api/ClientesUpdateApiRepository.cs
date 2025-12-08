using Microsoft.Extensions.Configuration;
using SAV.application.Repository;
using SAV.domain.Entities.Api;
using System.Text.Json;

namespace SAV.persistencia.Repositorios.Api
{
    public class ClientesUpdateApiRepository : IClientesUpdateApiRepo
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public ClientesUpdateApiRepository(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<IEnumerable<ClientesUpdate>> GetClientesUpdateAsync()
        {
            try
            {
                var apiUrl = _configuration["ExternalApi:ClientesUrl"];
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var clientes = JsonSerializer.Deserialize<List<ClientesUpdate>>(content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return clientes ?? new List<ClientesUpdate>();
                }

             
                return GetMockClientes();
            }
            catch (Exception)
            {
                return GetMockClientes();
            }
        }

        private IEnumerable<ClientesUpdate> GetMockClientes()
        {
            return new List<ClientesUpdate>
            {
                new ClientesUpdate { CustomerID = 1001, FirstName="Juan", LastName="Perez", Email="juan.perez@mail.com", Phone="809-000-0000", City="Santo Domingo", Country="Dominican Republic" },
                new ClientesUpdate { CustomerID = 1002, FirstName="Maria", LastName="Lopez", Email="maria.lopez@mail.com", Phone="809-111-1111", City="Santiago", Country="Dominican Republic" }
            };
        }
    }
}