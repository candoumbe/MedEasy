
using MedEasy.DAL.Interfaces;

namespace MedEasy.API.Stores
{
    public class EFUnitOfWork : UnitOfWork<MedEasyContext>
    {

        public EFUnitOfWork(MedEasyContext medEasyContext) : base(medEasyContext) {}
        
    }
}