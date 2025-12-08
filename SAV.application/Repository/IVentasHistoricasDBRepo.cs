using SAV.domain.Repository;
using SAV.domain.Entities.DB_Externa;

namespace SAV.application.Repository
{
    public interface IVentasHistoricasDBRepo 
    {
        Task<IEnumerable<VentasHistoricas>> GetVentasHistoricasAsync();
    }
}
