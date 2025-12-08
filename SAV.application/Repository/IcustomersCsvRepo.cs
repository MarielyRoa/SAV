using SAV.domain.Repository;
using SAV.domain.Entities.Csv;

namespace SAV.application.Repository
{
    public interface IcustomersCsvRepo : IReadRepository<customers>
    {
        Task<IEnumerable<object>> ReadFileAsync();
    }
}
