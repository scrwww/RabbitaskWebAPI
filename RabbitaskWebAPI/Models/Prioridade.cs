using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RabbitaskWebAPI.Models
{
    [Table("prioridade")]
    public partial class Prioridade
    {
        public Prioridade()
        {
            Tarefas = new HashSet<Tarefa>();
        }

        [Key]
        [Column("cd_prioridade")]
        public int CdPrioridade { get; set; }
        [Column("nm_prioridade")]
        [StringLength(250)]
        public string? NmPrioridade { get; set; }

        [InverseProperty("CdPrioridadeNavigation")]
        public virtual ICollection<Tarefa> Tarefas { get; set; }
    }
}
