using Microsoft.EntityFrameworkCore;
using ZadanieMFV.Models;

namespace ZadanieMFV.Database
{
    public class ProcessContext : DbContext
    {
        //przykladowo 
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ProcessStatusData>()
            .HasIndex(x => new { x.id }, "Process_Index");

            OnModelConfigurationEvents(modelBuilder);
        }
        static void OnModelConfigurationEvents(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProcessStatusData>();
        }

        protected override void OnConfiguring
                   (DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase(databaseName: "ProcessDb");
        }
       public DbSet<ProcessStatusData> ProcessStatus { get; set; }
        public DbSet<UserModel> Users { get; set; }
    }
}
