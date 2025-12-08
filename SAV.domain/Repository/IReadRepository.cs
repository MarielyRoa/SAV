
namespace SAV.domain.Repository
{
    public interface IReadRepository<T> where T : class
    {
        Task<IEnumerable<T>> ReadFileAsync(string archivo);
    }
}
