using Domain.Interfaces;

namespace Data.Persistence.Repositories
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        public Task<Guid> Add(T entity)
        {
            throw new NotImplementedException();
        }
    }
}
