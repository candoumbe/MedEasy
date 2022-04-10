namespace Identity.DataStores
{
    using Identity.Ids;
    using Identity.Objects;
    using Identity.ValueObjects;

    using MedEasy.DAL.Interfaces;

    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;

    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class IdentityRepository : IUserStore<Account>, IQueryableUserStore<Account>, IUserEmailStore<Account>
    {
        private readonly IdentityDataStore _store;

        public IdentityRepository(IdentityDataStore store)
        {
            _store = store;
        }

        ///<inheritdoc/>
        public IQueryable<Account> Users => _store.Set<Account>();

        ///<inheritdoc/>
        public async Task<IdentityResult> CreateAsync(Account user, CancellationToken cancellationToken)
        {
            _store.Set<Account>().Add(user);
            await _store.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            
            return IdentityResult.Success;
        }

        ///<inheritdoc/>
        public async Task<IdentityResult> DeleteAsync(Account user, CancellationToken cancellationToken)
        {
            _store.Set<Account>().Remove(user);
            await _store.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return IdentityResult.Success;
        }

        ///<inheritdoc/>
        public void Dispose()
        {
            _store?.Dispose();
            GC.SuppressFinalize(this);  
        }

        ///<inheritdoc/>
        public async Task<Account> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            Email email = Email.From(normalizedEmail);
            return await _store.Set<Account>().SingleOrDefaultAsync(u => u.Email == email, cancellationToken);
        }

        ///<inheritdoc/>
        public async Task<Account> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            AccountId accountId = (AccountId)Guid.Parse(userId);

            return await _store.Set<Account>().SingleOrDefaultAsync(account => account.Id == accountId, cancellationToken);

        }

        ///<inheritdoc/>
        public async Task<Account> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
            => await _store.Set<Account>().SingleOrDefaultAsync(account => account.Username == UserName.From(normalizedUserName), cancellationToken);

        public Task<string> GetEmailAsync(Account user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Email.Value);
        }

        ///<inheritdoc/>
        public Task<bool> GetEmailConfirmedAsync(Account user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.EmailConfirmed);
        }

        ///<inheritdoc/>
        public async Task<string> GetNormalizedEmailAsync(Account user, CancellationToken cancellationToken)
        {
            return await GetEmailAsync(user, cancellationToken).ConfigureAwait(false);
        }

        ///<inheritdoc/>
        public Task<string> GetNormalizedUserNameAsync(Account user, CancellationToken cancellationToken)
            => Task.FromResult(user.Username.Value);

        public Task<string> GetPasswordHashAsync(Account user, CancellationToken cancellationToken)
            => Task.FromResult(user.PasswordHash);

        ///<inheritdoc/>
        public Task<string> GetUserIdAsync(Account user, CancellationToken cancellationToken)
            => Task.FromResult(user.Id.Value.ToString());

        ///<inheritdoc/>
        public Task<string> GetUserNameAsync(Account user, CancellationToken cancellationToken)
            => Task.FromResult(user.Username.Value);

        ///<inheritdoc/>
        public Task<bool> HasPasswordAsync(Account user, CancellationToken cancellationToken)
            => Task.FromResult(user.PasswordHash is not null);

        ///<inheritdoc/>
        public Task SetEmailAsync(Account user, string email, CancellationToken cancellationToken)
        {
            user.ChangeEmail(Email.From(email));

            return Task.CompletedTask;
        }

        ///<inheritdoc/>
        public Task SetEmailConfirmedAsync(Account user, bool confirmed, CancellationToken cancellationToken)
        {
            if (confirmed)
            {
                user.ConfirmEmail();
            }
            else
            {
                user.UnconfirmEmail();
            }

            return Task.CompletedTask;
        }

        public Task SetNormalizedEmailAsync(Account user, string normalizedEmail, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        ///<inheritdoc/>
        public Task SetNormalizedUserNameAsync(Account user, string normalizedName, CancellationToken cancellationToken)
            => Task.CompletedTask;

        ///<inheritdoc/>
        public async Task SetUserNameAsync(Account user, string userName, CancellationToken cancellationToken)
        {
            _store.Attach(user); 
            user.ChangeUsernameTo(UserName.From(userName));
            await _store.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        ///<inheritdoc/>
        public async Task<IdentityResult> UpdateAsync(Account user, CancellationToken cancellationToken)
        {
            _store.Set<Account>().Update(user);
            await _store.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return IdentityResult.Success;
        }
    }
}
