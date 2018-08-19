using MedEasy.DataStores.Core.Relational;
using Identity.Objects;
using Microsoft.EntityFrameworkCore;

namespace Identity.DataStores.SqlServer
{
    /// <summary>
    /// Interacts with the underlying repostories.
    /// </summary>
    public class IdentityContext : DataStore<IdentityContext>
    {

        /// <summary>
        /// Collection of <see cref="Accounts"/>s
        /// </summary>
        public DbSet<Account> Accounts { get; set; }

        public DbSet<Claim> Claims { get; set; }

        /// <summary>
        /// Builds a new <see cref="IdentityContext"/> instance.
        /// </summary>
        /// <param name="options">options of the <see cref="IdentityContext"/></param>
        public IdentityContext(DbContextOptions<IdentityContext> options) : base(options)
        {
        }

        /// <summary>
        /// <see cref="DbContext.OnModelCreating(ModelBuilder)"/>
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<Account>(entity =>
            {
                entity.Property(x => x.UserName)
                    .HasMaxLength(NormalTextLength)
                    .IsRequired();

                entity.Property(x => x.Name)
                    .HasMaxLength(NormalTextLength);

                entity.HasIndex(x => x.UserName)
                    .IsUnique();
                entity.Property(x => x.Salt)
                    .IsRequired();

                entity.Property(x => x.PasswordHash)
                    .IsRequired();

                entity.HasMany(x => x.Roles);

                entity.Property(x => x.TenantId);
            });


            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasIndex(x => x.Code)
                    .IsUnique();
                    
                entity.Property(x => x.Code)
                    .HasMaxLength(ShortTextLength)
                    .IsRequired();

                entity.HasMany(x => x.Claims);
                entity.HasMany(x => x.Users);
            });




            modelBuilder.Entity<Claim>(entity =>
            {
                entity.HasIndex(x => x.Type)
                    .IsUnique();

                entity.Property(x => x.Type)
                    .HasMaxLength(ShortTextLength)
                    .IsRequired();
                entity.Property(x => x.Value)
                    .HasMaxLength(ShortTextLength)
                    .IsRequired();

                entity.HasMany(x => x.Roles);
            });

            modelBuilder.Entity<AccountClaim>(entity =>
            {
                entity.HasOne(x => x.Account)
                    .WithMany(x => x.Claims)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Claim)
                    .WithMany(claim => claim.Users);
            });




        }

    }
}
