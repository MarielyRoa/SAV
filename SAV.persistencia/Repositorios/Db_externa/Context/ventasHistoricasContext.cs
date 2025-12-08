using Microsoft.EntityFrameworkCore;
using SAV.domain.Entities.DB_Externa;

namespace SAV.persistencia.Repositorios.Db_externa.Context
{
    public class ventasHistoricasContext : DbContext
    {
        public ventasHistoricasContext(DbContextOptions<ventasHistoricasContext> options) 
            : base(options)
        {

        }
        public DbSet<VentasHistoricas> HistoricalSales { get; set; }
    }
}
