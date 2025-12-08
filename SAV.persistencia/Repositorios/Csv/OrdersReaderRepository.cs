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
    public sealed class OrdersReaderRepository : IordersCsvRepo
    {
        private readonly string _archivo;

        public OrdersReaderRepository(IConfiguration config)
        {
            _archivo = config["Csv:orders"];
        }

        public async Task<IEnumerable<orders>> ReadFileAsync(string archivo)
        {
            List<orders> resultado = new();

            using var reader = new StreamReader(_archivo);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            var records = csv.GetRecordsAsync<orders>();

            await foreach (var record in records)
                resultado.Add(record);

            return resultado; ;
        }
    }
}
