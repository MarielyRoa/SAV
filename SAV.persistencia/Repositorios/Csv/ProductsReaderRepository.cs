using CsvHelper;
using Microsoft.Extensions.Configuration;
using SAV.application.Repository;
using SAV.domain.Entities.Csv;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAV.persistencia.Repositorios.Csv
{
    public sealed class ProductsReaderRepository : IproductsCsvRepo
    {
        private readonly string _archivo;

        public ProductsReaderRepository(IConfiguration config)
        {
            _archivo = config["Csv:products"];
        }
        public async Task<IEnumerable<products>> ReadFileAsync(string archivo)
        {
            List<products> resultado = new();

            using var reader = new StreamReader(_archivo);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            var records = csv.GetRecordsAsync<products>();

            await foreach (var record in records)
                resultado.Add(record);

            return resultado;
        }
    }
}
