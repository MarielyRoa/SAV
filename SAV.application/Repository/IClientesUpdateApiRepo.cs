using SAV.domain.Repository;
using SAV.domain.Entities.Api;

namespace SAV.application.Repository
{
    public interface IClientesUpdateApiRepo 
    {
        Task<IEnumerable<ClientesUpdate>> GetClientesUpdateAsync();

    }
}
