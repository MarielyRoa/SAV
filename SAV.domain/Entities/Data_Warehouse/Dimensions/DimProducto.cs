using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SAV.domain.Entities.Data_Warehouse.Dimensions
{
    [Table("DimProducto", Schema = "Dimension")]
    public class DimProducto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductKey { get; set; }

        public int ProductID { get; set; }

        [Required, MaxLength(150)]
        public string ProductName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Category { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public int Stock { get; set; }

        [Required, MaxLength(100)]
        public string Marca { get; set; } = string.Empty;

        public DateTime FechaCarga { get; set; }

        // FK CORRECTA
        public int IdFuente { get; set; }
        [ForeignKey("IdFuente")]
        public DimFuente Fuente { get; set; }
    }
}
