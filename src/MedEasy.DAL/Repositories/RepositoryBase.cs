using System;
using MedEasy.DAL.Interfaces;

namespace MedEasy.DAL.Repositories
{
    public abstract class RepositoryBase<TEntry> where TEntry : class
    {
        
        protected IDbContext Context { get; private set; }

        protected RepositoryBase(IDbContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            Context = context;
        } 
    }

    
}