using Microsoft.EntityFrameworkCore;

namespace ZadanieMFV.Database
{
    public class ProcessContext : DbContext
    {

        protected override void OnConfiguring
                   (DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase(databaseName: "ProcessDb");
        }
      // public DbSet<> ProductModels { get; set; }
    }
}
