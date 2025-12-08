using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SAV.domain.Entities.Data_Warehouse.Dimensions;

namespace SAV.domain.Entities.Data_Warehouse.Dimensions
{
    [Table("DimCliente", Schema = "Dimension")]
    public class DimCliente
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CustomerKey { get; set; }

        [Required, MaxLength(50)]
        public int CustomerID { get; set; }

        [MaxLength(150)]
        public string FirstName { get; set; } = string.Empty;

        [MaxLength(150)]
        public string LastName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Phone { get; set; } = string.Empty;

        [MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Country { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Segmento { get; set; } = string.Empty;

        public DateTime FechaCarga { get; set; }

        // FK CORRECTA
        public int IdFuente { get; set; }
        [ForeignKey("IdFuente")]
        public DimFuente Fuente { get; set; }
    }
}
