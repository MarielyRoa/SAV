using Microsoft.AspNetCore.Mvc;
using SAV.application.Repository;
using SAV.domain.Entities.Api;
using System.Threading.Tasks;

namespace SAV.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientesApiController : ControllerBase
    {
        private readonly IClientesUpdateApiRepo _clientesRepo;

        public ClientesApiController(IClientesUpdateApiRepo clientesRepo)
        {
            _clientesRepo = clientesRepo;
        }

        [HttpGet("GetClientes")]
        public async Task<IActionResult> GetClientes()
        {
            var clientes = await _clientesRepo.GetClientesUpdateAsync();
            return Ok(clientes);
        }
    }
}
