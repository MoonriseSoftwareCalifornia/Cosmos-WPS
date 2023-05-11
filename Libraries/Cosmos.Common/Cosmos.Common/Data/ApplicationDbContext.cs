using Cosmos.Common.Data;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Cosmos.Common.Data
{
    /// <summary>
    ///     Database Context for Cosmos CMS
    /// </summary>
    public class ApplicationDbContext : AspNetCore.Identity.CosmosDb.CosmosIdentityDbContext<IdentityUser, IdentityRole>, IDataProtectionKeyContext
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="options"></param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        /// <summary>
        ///     Determine if this service is configured
        /// </summary>
        /// <returns></returns>
        public async Task<bool> IsConfigured()
        {
            return await base.Database.CanConnectAsync();
        }

        #region OVERRIDES

        /// <summary>
        ///     On model creating
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultContainer("CosmosCms");

            // Need to make a convertion so article number can be used as a partition key

            modelBuilder.Entity<ArticleNumber>()
                .ToContainer("ArticleNumber")
                .HasPartitionKey(k => k.Id)
                .HasKey(k => k.Id);

            modelBuilder.Entity<Article>()
                .Property(e => e.ArticleNumber)
                .HasConversion<string>();

            modelBuilder.Entity<Article>()
                .ToContainer("Articles")
                .HasPartitionKey(a => a.ArticleNumber)
                .HasKey(article => article.Id);

            modelBuilder.Entity<ArticleLock>()
                .ToContainer("ArticleLocks")
                .HasPartitionKey(a => a.Id)
                .HasKey(article => article.Id);

            modelBuilder.Entity<ArticleLog>()
                .ToContainer("ArticleLogs")
                .HasPartitionKey(k => k.Id)
                .HasKey(log => log.Id);

            modelBuilder.Entity<CatalogEntry>()
                .Property(e => e.ArticleNumber)
                .HasConversion<string>();

            modelBuilder.Entity<CatalogEntry>()
                .ToContainer("ArticleCatalog")
                .HasPartitionKey(k => k.ArticleNumber)
                .HasKey(log => log.ArticleNumber);

            modelBuilder.Entity<Layout>()
                .ToContainer("Layouts")
                .HasPartitionKey(a => a.Id)
                .HasKey(article => article.Id);

            modelBuilder.Entity<PublishedPage>()
                .ToContainer("Pages")
                .HasPartitionKey(a => a.UrlPath)
                .HasKey(article => article.Id);

            modelBuilder.Entity<Setting>()
                .ToContainer("Settings")
                .HasPartitionKey(a => a.Id)
                .HasKey(article => article.Id);

            modelBuilder.Entity<Template>()
                .ToContainer("Templates")
                .HasPartitionKey(k => k.Id)
                .HasKey(node => node.Id);

            base.OnModelCreating(modelBuilder);
        }

        #endregion

        #region DbContext


        /// <summary>
        /// Catalog of Articles
        /// </summary>
        public DbSet<CatalogEntry> ArticleCatalog { get; set; }

        /// <summary>
        /// Article locks
        /// </summary>
        public DbSet<ArticleLock> ArticleLocks { get; set; }

        /// <summary>
        ///     Article activity logs
        /// </summary>
        public DbSet<ArticleLog> ArticleLogs { get; set; }

        /// <summary>
        ///     Article Numbers
        /// </summary>
        public DbSet<ArticleNumber> ArticleNumbers { get; set; }

        /// <summary>
        ///     Articles
        /// </summary>
        public DbSet<Article> Articles { get; set; }

        /// <summary>
        /// Public information about article authors and editors
        /// </summary>
        public DbSet<AuthorInfo> AuthorInfos { get; set; }

        /// <summary>
        ///     Website layouts
        /// </summary>
        public DbSet<Layout> Layouts { get; set; }

        /// <summary>
        /// Node Scripts
        /// </summary>
        public DbSet<NodeScript> NodeScripts { get; set; }

        /// <summary>
        /// Published pages viewable via the publisher.
        /// </summary>
        public DbSet<PublishedPage> Pages { get; set; }

        /// <summary>
        /// Site settings.
        /// </summary>
        public DbSet<Setting> Settings { get; set; }

        /// <summary>
        ///     Web page templates
        /// </summary>
        public DbSet<Template> Templates { get; set; }

        /// <summary>
        /// Data protection keys
        /// </summary>
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;

        #endregion
    }
}