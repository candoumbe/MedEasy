using Documents.Objects;
using MedEasy.DataStores.Core.Relational;
using Microsoft.EntityFrameworkCore;
using System;

namespace Documents.DataStore.SqlServer
{
    public class DocumentsStore : DataStore<DocumentsStore>
    {
        public DbSet<Document> Documents { get; set; }

        public DocumentsStore(DbContextOptions<DocumentsStore> options) : base(options)
        {
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Document>(entity =>
            {
                entity.OwnsOne(x => x.File, (file) =>
                {

                    file.ToTable(nameof(Document.File));
                    file.Property(f => f.Content)
                        .IsRequired();

                });
                
                entity.Property(x => x.Name)
                    .HasMaxLength(255)
                    .IsRequired();
                entity.Property(x => x.MimeType)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasDefaultValue(Document.DefaultMimeType);


            });
        }
    }
}
