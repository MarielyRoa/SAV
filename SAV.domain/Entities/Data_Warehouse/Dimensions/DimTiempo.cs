using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SAV.domain.Entities.Data_Warehouse.Dimensions
{
    [Table("DimTiempo", Schema = "Dimension")]
    public class DimTiempo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TiempoKey { get; set; }

        [Required]
        public DateTime Fecha { get; set; }

        public int Año { get; set; }
        public int Mes { get; set; }

        [Required, MaxLength(20)]
        public string nombre_mes { get; set; } = string.Empty;

        public int trimestre { get; set; }
        public int semestre { get; set; }
        public int semana_año { get; set; }
        public int dia_mes { get; set; }

        [Required, MaxLength(20)]
        public string dia_semana { get; set; } = string.Empty;

        public bool es_fin_semana { get; set; }

        public bool es_feriado { get; set; }

        [Required, MaxLength(7)]
        public string mes_año { get; set; } = string.Empty;
    }
}
