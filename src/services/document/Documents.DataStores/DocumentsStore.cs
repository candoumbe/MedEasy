namespace Documents.DataStore
{
    using Documents.Objects;

    using MedEasy.DataStores.Core.Relational;

    using Microsoft.EntityFrameworkCore;

    using NodaTime;

    public class DocumentsStore : DataStore<DocumentsStore>
    {
        public DbSet<Document> Documents { get; set; }

        public DocumentsStore(DbContextOptions<DocumentsStore> options, IClock clock) : base(options, clock)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Document>(entity =>
            {
                entity.HasMany(x => x.Parts)
                      .WithOne()
                      .HasForeignKey(part => part.DocumentId)
                      .HasPrincipalKey(doc => doc.Id);

                entity.Property(x => x.Status)
                      .HasConversion<string>()
                      .HasDefaultValue(Status.Ongoing);

                entity.Property(x => x.Name)
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(x => x.MimeType)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasDefaultValue(Document.DefaultMimeType);
            });

            modelBuilder.Entity<DocumentPart>(file =>
            {
                file.HasKey(x => new { x.DocumentId, x.Position });

                file.Property(f => f.Content)
                    .IsRequired();
            });
        }
    }
}
