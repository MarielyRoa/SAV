using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SAV.domain.Entities.DB_Externa
{
    [Table("HistoricalSales")]
    public class VentasHistoricas
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // si OrderID viene definido desde la BD externa
        public int OrderID { get; set; }

        public int CustomerID { get; set; }
        public int ProductID { get; set; }
        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        public DateTime OrderDate { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = string.Empty;
    }
}
