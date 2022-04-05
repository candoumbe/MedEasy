namespace Identity.DataStores
{
    using Identity.Ids;
    using Identity.Objects;
    using Identity.ValueObjects;

    using MedEasy.DAL.Interfaces;

    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;

    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class IdentityRepository : IUserStore<Account>
    {
        private readonly IdentityDataStore _store;

        public IdentityRepository(IdentityDataStore store)
        {
            _store = store;
        }

        ///<inheritdoc/>
        public async Task<IdentityResult> CreateAsync(Account user, CancellationToken cancellationToken)
        {
            _store.Set<Account>().Add(user);
            return IdentityResult.Success;
        }

        ///<inheritdoc/>
        public async Task<IdentityResult> DeleteAsync(Account user, CancellationToken cancellationToken)
        {
            _store.Set<Account>().Remove(user);
            return IdentityResult.Success;
        }

        ///<inheritdoc/>
        public void Dispose()
        {
            _store?.Dispose();
            GC.SuppressFinalize(this);  
        }

        ///<inheritdoc/>
        public async Task<Account> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            AccountId accountId = (AccountId)Guid.Parse(userId);

            return await _store.Set<Account>().SingleOrDefaultAsync(account => account.Id == accountId, cancellationToken);

        }

        public Task<Account> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetNormalizedUserNameAsync(Account user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetUserIdAsync(Account user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetUserNameAsync(Account user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetNormalizedUserNameAsync(Account user, string normalizedName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetUserNameAsync(Account user, string userName, CancellationToken cancellationToken)
        {
            user.ChangeUsernameTo(UserName.From(userName));
            return Task.CompletedTask;
        }

        public Task<IdentityResult> UpdateAsync(Account user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
