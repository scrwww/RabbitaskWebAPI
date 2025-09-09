using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RabbitaskWebAPI.Models
{
    [Table("tarefa_tag")]
    [Index("CdTarefa", Name = "fk_tarefa_tag_tarefa")]
    public partial class TarefaTag
    {
        [Key]
        [Column("cd_tag")]
        public int CdTag { get; set; }
        [Key]
        [Column("cd_tarefa")]
        public int CdTarefa { get; set; }

        [ForeignKey("CdTag")]
        [InverseProperty("TarefaTags")]
        public virtual Tag CdTagNavigation { get; set; } = null!;
    }
}
