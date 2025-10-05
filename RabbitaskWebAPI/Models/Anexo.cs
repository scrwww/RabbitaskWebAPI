using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RabbitaskWebAPI.Models
{
    [Table("anexo")]
    [Index("CdUsuario", "CdTarefa", Name = "fk_anexo_tarefa")]
    public partial class Anexo
    {
        [Key]
        [Column("cd_anexo")]
        public int CdAnexo { get; set; }
        [Key]
        [Column("cd_usuario")]
        public int CdUsuario { get; set; }
        [Key]
        [Column("cd_tarefa")]
        public int CdTarefa { get; set; }
        [Column("nm_caminho_anexo")]
        [StringLength(500)]
        public string? NmCaminhoAnexo { get; set; }

        [ForeignKey("CdUsuario,CdTarefa")]
        [InverseProperty("Anexos")]
        public virtual Tarefa Cd { get; set; } = null!;
    }
}
