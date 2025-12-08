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
    public sealed class Order_detailsReaderRepository : Iorder_detailsCsvRepo
    {
        private readonly string _archivo;

        public Order_detailsReaderRepository(IConfiguration config)
        {
            _archivo = config["Csv:order_details"];
        }
        public async Task<IEnumerable<order_details>> ReadFileAsync(string archivo)
        {
            List<order_details> resultado = new();

            using var reader = new StreamReader(_archivo);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            var records = csv.GetRecordsAsync<order_details>();

            await foreach (var record in records)
                resultado.Add(record);

            return resultado;
        }
    }
}
