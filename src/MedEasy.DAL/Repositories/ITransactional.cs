using System.Threading;
using System.Threading.Tasks;

namespace MedEasy.DAL.Repositories
{
    public interface ITransactional
    {
        int SaveChanges();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));
        
    }
}
