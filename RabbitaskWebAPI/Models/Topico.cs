using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RabbitaskWebAPI.Models
{
    [Table("topico")]
    [Index("CdTarefa", Name = "fk_topico_tarefa")]
    public partial class Topico
    {
        [Key]
        [Column("cd_topico")]
        public int CdTopico { get; set; }
        [Column("nm_topico")]
        [StringLength(250)]
        public string? NmTopico { get; set; }
        [Column("cd_tarefa")]
        public int? CdTarefa { get; set; }
        [Column("ic_concluido")]
        public bool? IcConcluido { get; set; }

        [ForeignKey("CdTarefa")]
        [InverseProperty("Topicos")]
        public virtual Tarefa? CdTarefaNavigation { get; set; }
    }
}
