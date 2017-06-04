using MedEasy.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MedEasy.Search.API.Context
{
    /// <summary>
    /// DbContext for the API
    /// </summary>
    public class MedEasySearchContext : DbContext, IDbContext
    {

        /// <summary>
        /// Builds a new instance of <see cref="MedEasySearchContext"/> with default options
        /// </summary>
        public MedEasySearchContext()
        {
        }


        /// <summary>
        /// Builds à new instance of <see cref="MedEasySearchContext"/> with the specified <see cref="DbContextOptions{TContext}"/>
        /// </summary>
        /// <param name="options">options to customize the context behaviour</param>
        public MedEasySearchContext(DbContextOptions<MedEasySearchContext> options) : base(options)
        {
        }

        ///<inheritdoc/>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=MedEasySearchDb;Trusted_Connection=True;MultipleActiveResultSets=true");
            }

            base.OnConfiguring(optionsBuilder);

        }
        

    }
}
