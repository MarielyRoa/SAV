using CsvHelper;
using Microsoft.Extensions.Configuration;
using SAV.application.Repository;
using SAV.domain.Entities.Csv;
using System.Globalization;

namespace SAV.persistencia.Repositorios.Csv
{
    public sealed class CustomersReaderRepository : IcustomersCsvRepo
    {
        private readonly string _archivo;

        public CustomersReaderRepository(IConfiguration config)
        {
            _archivo = config["Csv:customers"];
        }

        public Task<IEnumerable<object>> ReadFileAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<customers>> ReadFileAsync(string archivo)
        {
            List<customers> resultado = new();

            using var reader = new StreamReader(_archivo);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            var records = csv.GetRecordsAsync<customers>();

            await foreach (var record in records)
                resultado.Add(record);

            return resultado;

        }
    }
}


