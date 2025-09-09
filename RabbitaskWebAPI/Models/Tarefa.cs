using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RabbitaskWebAPI.Models
{
    [Table("tarefa")]
    [Index("CdPrioridade", Name = "fk_tarefa_prioridade")]
    [Index("CdUsuario", Name = "fk_tarefa_usuario")]
    [Index("CdUsuarioProprietario", Name = "fk_tarefa_usuario_proprietario")]
    public partial class Tarefa
    {
        public Tarefa()
        {
            Anexos = new HashSet<Anexo>();
            TarefaFeedbacks = new HashSet<TarefaFeedback>();
            Topicos = new HashSet<Topico>();
            CdTags = new HashSet<Tag>();
        }

        [Key]
        [Column("cd_tarefa")]
        public int CdTarefa { get; set; }
        [Column("nm_tarefa")]
        [StringLength(250)]
        public string? NmTarefa { get; set; }
        [Column("cd_prioridade")]
        public int? CdPrioridade { get; set; }
        [Column("ds_tarefa")]
        [StringLength(2000)]
        public string? DsTarefa { get; set; }
        [Column("dt_prazo", TypeName = "datetime")]
        public DateTime? DtPrazo { get; set; }
        [Column("cd_usuario_proprietario")]
        public int? CdUsuarioProprietario { get; set; }
        [Column("cd_usuario")]
        public int? CdUsuario { get; set; }
        [Column("dt_criacao", TypeName = "datetime")]
        public DateTime? DtCriacao { get; set; }
        [Column("dt_conclusao", TypeName = "datetime")]
        public DateTime? DtConclusao { get; set; }

        [ForeignKey("CdPrioridade")]
        [InverseProperty("Tarefas")]
        public virtual Prioridade? CdPrioridadeNavigation { get; set; }
        [ForeignKey("CdUsuario")]
        [InverseProperty("TarefaCdUsuarioNavigations")]
        public virtual Usuario? CdUsuarioNavigation { get; set; }
        [ForeignKey("CdUsuarioProprietario")]
        [InverseProperty("TarefaCdUsuarioProprietarioNavigations")]
        public virtual Usuario? CdUsuarioProprietarioNavigation { get; set; }
        [InverseProperty("CdTarefaNavigation")]
        public virtual ICollection<Anexo> Anexos { get; set; }
        [InverseProperty("CdTarefaNavigation")]
        public virtual ICollection<TarefaFeedback> TarefaFeedbacks { get; set; }
        [InverseProperty("CdTarefaNavigation")]
        public virtual ICollection<Topico> Topicos { get; set; }

        [ForeignKey("CdTarefa")]
        [InverseProperty("CdTarefas")]
        public virtual ICollection<Tag> CdTags { get; set; }
    }
}
