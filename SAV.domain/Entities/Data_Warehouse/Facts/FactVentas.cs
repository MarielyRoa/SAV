using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SAV.domain.Entities.Data_Warehouse.Dimensions;

namespace SAV.domain.Entities.Data_Warehouse.Facts
{
    [Table("FactVentas", Schema = "Fact")]
    public class FactVentas
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int VentaKey { get; set; }

        // FK → DimTiempo
        [Column("TiempoKey")]
        public int TiempoKey { get; set; }

        [ForeignKey(nameof(TiempoKey))]
        public DimTiempo DimTiempo { get; set; }

        // FK → DimProducto
        [Column("ProductKey")]
        public int ProductKey { get; set; }

        [ForeignKey(nameof(ProductKey))]
        public DimProducto DimProducto { get; set; }

        // FK → DimCliente
        [Column("CustomerKey")]
        public int CustomerKey { get; set; }

        [ForeignKey(nameof(CustomerKey))]
        public DimCliente DimCliente { get; set; }

        // FK → DimFuente
        [Column("FuenteKey")]
        public int FuenteKey { get; set; }

        [ForeignKey(nameof(FuenteKey))]
        public DimFuente DimFuente { get; set; }

        // Métricas de la venta
        public int cantidad { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal precio_unitario { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal total_venta { get; set; }

        public DateTime fecha_carga { get; set; }

        [Required]
        [MaxLength(50)]
        public string OrderID { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; }
    }
}
