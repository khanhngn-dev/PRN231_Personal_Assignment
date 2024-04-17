
using Repositorty.BaseRepository;

namespace Repositorty.UnitOfWork;

public interface IUnitOfWork : IDisposable
{
    IRepository<TEntity> GetRepository<TEntity>() where TEntity : class;
    void Save();
}