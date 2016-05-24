using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MedEasy.DAL.Repositories;

namespace MedEasy.DAL.Interfaces
{
    public class UnitOfWork<TContext> : IUnitOfWork where TContext : IDbContext
    {
        private readonly TContext _context;
        private readonly IDictionary<Type, object> _repositories;
        private bool _disposed;


        public UnitOfWork(TContext context)
        {
            _context = context;
            _repositories = new Dictionary<Type, object>();
            _disposed = false;
        }

        public IRepository<TEntry> Repository<TEntry>() where TEntry : class
        {
            IRepository<TEntry> repository;
            // Checks if the Dictionary Key contains the Model class
            if (_repositories.Keys.Contains(typeof(TEntry)))
            {
                // Return the repository for that Model class
                repository = _repositories[typeof(TEntry)] as IRepository<TEntry>;
            }
            else
            {
                // If the repository for that Model class doesn't exist, create it
                repository = new Repository<TEntry>(_context);
                // Add it to the dictionary
                _repositories.Add(typeof(TEntry), repository);
            }

            return repository;
        }
        
        /// <summary>
        /// Saves all pending changes
        /// </summary>
        /// <returns>The number of objects in an Added, Modified, or Deleted state</returns>
        public int SaveChanges()
        {
            return _context.SaveChanges();
        }

        /// <summary>
        /// Saves all pending changes
        /// </summary>
        /// <returns>The number of objects in an Added, Modified, or Deleted state</returns>
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Saves all pending changes
        /// </summary>
        /// <returns>The number of objects in an Added, Modified, or Deleted state</returns>
        public async Task<int> SaveChangesAsync(CancellationToken token)
        {
            return await _context.SaveChangesAsync(token);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the current object
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context?.Dispose();
                }

                _disposed = true;
            }
        }
    }
}