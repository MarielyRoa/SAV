using SAV.application.Resultado;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAV.application.Interfaces
{
    public interface IDataWarehouseService
    {
        Task<Result> ProcessDimensionsLoadAsync();
    }
}
