using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RabbitaskWebAPI.Models
{
    [Table("tipo_sentimento")]
    public partial class TipoSentimento
    {
        public TipoSentimento()
        {
            TarefaFeedbacks = new HashSet<TarefaFeedback>();
        }

        [Key]
        [Column("cd_tipo_sentimento")]
        public int CdTipoSentimento { get; set; }
        [Column("nm_tipo_sentimento")]
        [StringLength(250)]
        public string? NmTipoSentimento { get; set; }

        [InverseProperty("CdTipoSentimentoNavigation")]
        public virtual ICollection<TarefaFeedback> TarefaFeedbacks { get; set; }
    }
}
