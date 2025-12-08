using Microsoft.AspNetCore.Mvc;
using SAV.application.Repository;
using SAV.domain.Entities.Api;
using System.Threading.Tasks;

namespace SAV.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductosApiController : ControllerBase
    {
        private readonly IProductosUpdateApiRepo _productosRepo;

        public ProductosApiController(IProductosUpdateApiRepo productosRepo)
        {
            _productosRepo = productosRepo;
        }

        [HttpGet("GetProductos")]
        public async Task<IActionResult> GetProductos()
        {
            var productos = await _productosRepo.GetProductosUpdateAsync();
            return Ok(productos);
        }
    }
}
