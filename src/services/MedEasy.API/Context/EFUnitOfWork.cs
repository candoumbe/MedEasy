
using MedEasy.DAL.Interfaces;

namespace MedEasy.API.Stores
{
    /// <summary>
    /// Unit of work implementation that relies on Entity Framework.
    /// </summary>
    public class EFUnitOfWork : UnitOfWork<MedEasyContext>
    {
        /// <summary>
        /// Builds a new <see cref="EFUnitOfWork"/> instance.
        /// </summary>
        /// <param name="medEasyContext">The context used by this unit of work instance.</param>
        public EFUnitOfWork(MedEasyContext medEasyContext) : base(medEasyContext) {}
        
    }
}