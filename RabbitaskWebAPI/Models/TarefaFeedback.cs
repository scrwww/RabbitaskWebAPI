using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RabbitaskWebAPI.Models
{
    [Table("tarefa_feedback")]
    [Index("CdUsuario", "CdTarefa", Name = "fk_tarefa_feedback_tarefa")]
    public partial class TarefaFeedback
    {
        [Key]
        [Column("cd_tipo_sentimento")]
        public int CdTipoSentimento { get; set; }
        [Key]
        [Column("cd_usuario")]
        public int CdUsuario { get; set; }
        [Key]
        [Column("cd_tarefa")]
        public int CdTarefa { get; set; }
        [Column("ds_feedback")]
        [StringLength(2000)]
        public string? DsFeedback { get; set; }

        [ForeignKey("CdUsuario,CdTarefa")]
        [InverseProperty("TarefaFeedbacks")]
        public virtual Tarefa Cd { get; set; } = null!;
        [ForeignKey("CdTipoSentimento")]
        [InverseProperty("TarefaFeedbacks")]
        public virtual TipoSentimento CdTipoSentimentoNavigation { get; set; } = null!;
    }
}
