using Microsoft.EntityFrameworkCore;
using SAV.domain.Entities.Data_Warehouse.Dimensions;
using SAV.domain.Entities.Data_Warehouse.Facts;

namespace SAV.persistencia.Repositorios.Data_Warehouse.Context
{
    public class DtwarehouseContext : DbContext
    {
        public DtwarehouseContext(DbContextOptions<DtwarehouseContext> options) : base(options)
        {
        }

        public DbSet<DimCliente> DimClientes { get; set; }
        public DbSet<DimProducto> DimProductos { get; set; }
        public DbSet<FactVentas> FactVentas { get; set; }
        public DbSet<DimFuente> DimFuentes { get; set; }
        public DbSet<DimTiempo> DimTiempos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(string))
                    {
                        property.SetIsUnicode(true); 
                    }
                }
            }

            // -------------------------------
            // DimCliente
            // -------------------------------
            modelBuilder.Entity<DimCliente>(entity =>
            {
                entity.ToTable("DimCliente", "Dimension");
                entity.HasKey(e => e.CustomerKey);

                entity.Property(e => e.CustomerKey)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(true);

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(true);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(true);

                entity.Property(e => e.Phone)
                    .IsRequired()
                    .HasMaxLength(25)
                    .IsUnicode(true);

                entity.Property(e => e.City)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(true);

                entity.Property(e => e.Country)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(true);

                entity.Property(e => e.Segmento)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(true);

                entity.Property(e => e.FechaCarga)
                    .HasColumnType("datetime2");
            });

            // -------------------------------
            // DimProducto
            // -------------------------------
            modelBuilder.Entity<DimProducto>(entity =>
            {
                entity.ToTable("DimProducto", "Dimension");
                entity.HasKey(e => e.ProductKey);

                entity.Property(e => e.ProductKey)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.ProductName)
                    .IsRequired()
                    .HasMaxLength(150)
                    .IsUnicode(true);

                entity.Property(e => e.Category)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(true);

                entity.Property(e => e.Price)
                    .HasPrecision(18, 2);

                entity.Property(e => e.Marca)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(true);
            });

            // -------------------------------
            // DimFuente
            // -------------------------------
            modelBuilder.Entity<DimFuente>(entity =>
            {
                entity.ToTable("DimFuente", "Dimension");
                entity.HasKey(e => e.FuenteKey);

                entity.Property(e => e.FuenteKey)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.TipoFuente)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(true);

                entity.Property(e => e.Descripcion)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(true);
            });

            // -------------------------------
            // DimTiempo
            // -------------------------------
            modelBuilder.Entity<DimTiempo>(entity =>
            {
                entity.ToTable("DimTiempo", "Dimension");
                entity.HasKey(e => e.TiempoKey);

                entity.Property(e => e.TiempoKey)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.nombre_mes)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(true);

                entity.Property(e => e.dia_semana)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(true);

                entity.Property(e => e.mes_año)
                    .IsRequired()
                    .HasMaxLength(7)
                    .IsUnicode(true);
            });

            // -------------------------------
            // FactVentas
            // -------------------------------
            modelBuilder.Entity<FactVentas>(entity =>
            {
                entity.ToTable("FactVentas", "Fact");
                entity.HasKey(e => e.VentaKey);

                entity.Property(e => e.VentaKey)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.precio_unitario)
                    .HasPrecision(18, 2);

                entity.Property(e => e.total_venta)
                    .HasPrecision(18, 2);

                entity.Property(e => e.OrderID)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(true);

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(true)
                    .HasDefaultValue("Pending");

                // Relaciones con dimensiones
                entity.HasOne(f => f.DimCliente)
                      .WithMany()
                      .HasForeignKey(f => f.CustomerKey)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(f => f.DimProducto)
                      .WithMany()
                      .HasForeignKey(f => f.ProductKey)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(f => f.DimFuente)
                      .WithMany()
                      .HasForeignKey(f => f.FuenteKey)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(f => f.DimTiempo)
                      .WithMany()
                      .HasForeignKey(f => f.TiempoKey)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}