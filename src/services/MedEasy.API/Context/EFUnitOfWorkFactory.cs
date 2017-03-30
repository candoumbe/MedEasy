using MedEasy.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MedEasy.API.Stores
{
    /// <summary>
    /// Defines a unit of work factory wrapper around Entity Framework
    /// </summary>
    public class EFUnitOfWorkFactory : UnitOfWorkFactory
    {
        /// <summary>
        /// Options used to create the unit of work instances
        /// </summary>
        public DbContextOptions<MedEasyContext> Options { get; }

        /// <summary>
        /// Builds a new <see cref="EFUnitOfWorkFactory"/> instance
        /// </summary>
        /// <param name="options">options that will be used by the <see cref="EFUnitOfWork"/> returned by calling <see cref="New"/></param>
        public EFUnitOfWorkFactory(DbContextOptions<MedEasyContext> options)
        {
            Options = options;
        }

        /// <summary>
        /// Creates new <see cref="EFUnitOfWork"/> instances.
        /// </summary>
        /// <remarks>
        /// Each call returns a new instance of <see cref="EFUnitOfWork"/> (which wraps its own <see cref="DbContext"/> instance) 
        /// that can safely be used in multithreaded fashion.
        /// </remarks>
        /// <returns><see cref="EFUnitOfWork"/> instance</returns>
        public override IUnitOfWork New() => new EFUnitOfWork(new MedEasyContext(Options));
    }
}