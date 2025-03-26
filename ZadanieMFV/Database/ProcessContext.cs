using Microsoft.EntityFrameworkCore;
using ZadanieMFV.Models;

namespace ZadanieMFV.Database
{
    public class ProcessContext : DbContext
    {

        protected override void OnConfiguring
                   (DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase(databaseName: "ProcessDb");
        }
       public DbSet<ProcessStatusData> ProcessStatus { get; set; }
        public DbSet<UserModel> Users { get; set; }
    }
}
