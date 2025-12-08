using Microsoft.EntityFrameworkCore;
using SAV.application.Repository;
using SAV.domain.Entities.DB_Externa;
using SAV.persistencia.Repositorios.Db_externa.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAV.persistencia.Repositorios.Db_externa
{
    public class VentasHistoricasDbRepository : IVentasHistoricasDBRepo
    {
        private readonly ventasHistoricasContext _context;

        public VentasHistoricasDbRepository(ventasHistoricasContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<VentasHistoricas>> GetVentasHistoricasAsync()
        {
            return await _context.HistoricalSales.ToListAsync();
        }
    }
}
