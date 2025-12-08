
using SAV.domain.Repository;
using SAV.domain.Entities.Api;

namespace SAV.application.Repository
{
    public interface IProductosUpdateApiRepo
    {
        Task<IEnumerable<ProductosUpdate>> GetProductosUpdateAsync();

    }
}
