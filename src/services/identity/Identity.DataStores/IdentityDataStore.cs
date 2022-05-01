namespace Identity.DataStores
{
    using Identity.Objects;
    using MedEasy.ValueObjects;

    using MedEasy.DataStores.Core.Relational;

    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;

    using NodaTime;

    /// <summary>
    /// Interacts with the underlying repostories.
    /// </summary>
    public class IdentityDataStore : DataStore<IdentityDataStore>
    {
        /// <summary>
        /// Builds a new <see cref="IdentityDataStore"/> instance.
        /// </summary>
        /// <param name="options">options of the <see cref="IdentityDataStore"/></param>
        public IdentityDataStore(DbContextOptions<IdentityDataStore> options, IClock clock) : base(options, clock)
        {
        }

        /// <summary>
        /// <see cref="DbContext.OnModelCreating(ModelBuilder)"/>
        /// </summary>
        /// <param name="builder"></param>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Account>(entity =>
            {
                entity.Property(x => x.Username)
                    .HasMaxLength(NormalTextLength)
                    .IsRequired()
                    .HasConversion(convertToProviderExpression: x => x.Value,
                                   convertFromProviderExpression: x => UserName.From(x)
                      );

                entity.Property(x => x.Name)
                    .HasMaxLength(NormalTextLength);

                entity.HasIndex(x => x.Username)
                    .IsUnique();
                entity.Property(x => x.Salt)
                    .IsRequired();

                entity.Property(x => x.PasswordHash)
                    .HasConversion(convertToProviderExpression: x => x.Value,
                                   convertFromProviderExpression: x => Password.From(x)
                      )
                    .IsRequired();

                entity.Property(x => x.TenantId);

                entity.Property(x => x.Email)
                      .HasConversion<string>(convertToProviderExpression: x => x.Value,
                                             convertFromProviderExpression: x => Email.From(x)
                      );

                entity.Property(x => x.IsActive);

                entity.Property(x => x.Locked);

                entity.HasMany(x => x.Roles);
                entity.HasMany(x => x.Claims)
                      .WithOne()
                      .HasForeignKey(accountClaim => accountClaim.AccountId)
                      .HasPrincipalKey(account => account.Id);
            });

            builder.Entity<AccountClaim>(entity =>
            {
                entity.HasKey(x => new { x.AccountId, x.Id });
                entity.Property(x => x.Start);
                entity.Property(x => x.End);
                entity.OwnsOne(x => x.Claim, claim =>
                {
                    claim.Property(x => x.Type)
                        .HasMaxLength(NormalTextLength)
                        .IsRequired();

                    claim.Property(x => x.Value)
                        .HasMaxLength(NormalTextLength)
                        .IsRequired();
                });
            });

            builder.Entity<AccountRole>(entity =>
            {
                entity.HasKey(x => new { x.AccountId, x.RoleId });

                entity.HasOne(ar => ar.Role)
                      .WithMany(role => role.Accounts)
                      .HasForeignKey(ar => ar.RoleId)
                      .HasPrincipalKey(role => role.Id);

                entity.HasOne(ar => ar.Account)
                      .WithMany(account => account.Roles)
                      .HasForeignKey(ar => ar.AccountId)
                      .HasPrincipalKey(account => account.Id);
            });

            builder.Entity<RoleClaim>(entity =>
            {
                entity.HasKey(x => new { x.RoleId, x.Id });
                entity.OwnsOne(x => x.Claim, claim =>
                {
                    claim.Property(x => x.Type)
                        .HasMaxLength(NormalTextLength)
                        .IsRequired();

                    claim.Property(x => x.Value)
                        .HasMaxLength(NormalTextLength)
                        .IsRequired();
                });
            });


            builder.Entity<Role>(entity =>
            {
                entity.HasIndex(x => x.Code)
                   .IsUnique();

                entity.Property(x => x.Code)
                    .HasMaxLength(ShortTextLength)
                    .IsRequired();

                entity.HasMany(x => x.Claims)
                      .WithOne()
                      .HasForeignKey(roleClaim => roleClaim.RoleId)
                      .HasPrincipalKey(role => role.Id);
                entity.HasMany(x => x.Accounts);
            });
        }
    }
}
