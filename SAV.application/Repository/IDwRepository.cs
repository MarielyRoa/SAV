using SAV.application.Resultado;
using SAV.domain.Entities.Data_Warehouse.Dimensions;
using SAV.domain.Entities.Data_Warehouse.Facts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAV.application.Repository
{
    public interface IDwRepository
    {
        Task<Result> LoadDimsDataAsync();
        Task<Result> LoadFactsDataAsync(); // NUEVO

        Task<int> InsertDimClienteAsync(DimCliente cliente);
        Task<int> InsertDimProductoAsync(DimProducto producto);
        Task<int> InsertDimFuenteAsync(DimFuente fuente);
        Task<int> InsertDimTiempoAsync(DimTiempo tiempo);
        Task<int> InsertFactVentaAsync(FactVentas fact);
    }
}
