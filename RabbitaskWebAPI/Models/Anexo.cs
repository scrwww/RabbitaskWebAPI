using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RabbitaskWebAPI.Models
{
    [Table("anexo")]
    [Index("CdTarefa", Name = "fk_anexo_tarefa")]
    public partial class Anexo
    {
        [Key]
        [Column("cd_anexo")]
        public int CdAnexo { get; set; }
        [Column("cd_tarefa")]
        public int? CdTarefa { get; set; }
        [Column("nm_caminho_anexo")]
        [StringLength(500)]
        public string? NmCaminhoAnexo { get; set; }

        [ForeignKey("CdTarefa")]
        [InverseProperty("Anexos")]
        public virtual Tarefa? CdTarefaNavigation { get; set; }
    }
}
