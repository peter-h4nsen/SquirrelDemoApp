using SquirrelDemoApp.Entities;
using System;
using System.Data.Entity;

namespace SquirrelDemoApp.DAL
{
    public sealed class TestDbContext : DbContext
    {
        static TestDbContext()
        {
            Database.SetInitializer<TestDbContext>(null);
        }

        public TestDbContext(string connectionstring)
            : base(connectionstring)
        {
            Configuration.LazyLoadingEnabled = false;
            Configuration.ProxyCreationEnabled = false;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FirstTable>().ToTable("FirstTable");
        }

        public DbSet<FirstTable> FirstTableDbSet { get; set; }
    }
}
