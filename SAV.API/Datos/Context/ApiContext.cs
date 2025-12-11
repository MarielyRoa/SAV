using SAV.domain.Entities.Api;
using Microsoft.EntityFrameworkCore;

namespace SAV.api.Data.Context
{
    public class ApiContext : DbContext
    {
        public ApiContext(DbContextOptions<ApiContext> options) : base(options)
        {
        }

        public DbSet<ClientesUpdate> Clientes { get; set; }
        public DbSet<ProductosUpdate> Productos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ClientesUpdate>().HasKey(c => c.CustomerID);
            modelBuilder.Entity<ProductosUpdate>().HasKey(p => p.ProductID);
        }
    }
}
