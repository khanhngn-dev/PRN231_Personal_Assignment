
using BusinessObjects;
using Repositorty.BaseRepository;

namespace Repositorty.UnitOfWork;

public class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly PRN231_SU23_StudentGroupDBContext _context;

    public UnitOfWork(PRN231_SU23_StudentGroupDBContext context)
    {
        _context = context;
    }

    public IRepository<TEntity> GetRepository<TEntity>() where TEntity : class
        => new Repository<TEntity>(_context);

    public void Save()
    {
        _context.SaveChanges();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _context.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}