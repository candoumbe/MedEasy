using Identity.CQRS.Queries.Accounts;
using Identity.Objects;

using MedEasy.DAL.Interfaces;

using MediatR;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.CQRS.Handlers.Queries.Accounts
{
    /// <summary>
    /// Handles queries to check if an <see cref="DTO.AccountInfo"/>'s <see cref="DTO.AccountInfo.Id"/> stands for a tenant.
    /// </summary>
    public class HandleIsTenantQuery : IRequestHandler<IsTenantQuery, bool>
    {
        private readonly IUnitOfWorkFactory _uowFactory;

        /// <summary>
        /// Builds a new <see cref="HandleIsTenantQuery"/> instance.
        /// </summary>
        /// <param name="uowFactory"></param>
        /// <exception cref="ArgumentNullException">if <paramref name="uowFactory"/> is <c>null</c>.</exception>
        public HandleIsTenantQuery(IUnitOfWorkFactory uowFactory) => _uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));


        public async Task<bool> Handle(IsTenantQuery request, CancellationToken ct)
        {
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                return await uow.Repository<Account>()
                    .AnyAsync(x => x.TenantId == request.Data)
                    .ConfigureAwait(false);
            }
        }
    }
}
