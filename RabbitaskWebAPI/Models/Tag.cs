using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RabbitaskWebAPI.Models
{
    [Table("tag")]
    public partial class Tag
    {
        public Tag()
        {
            Cds = new HashSet<Tarefa>();
        }

        [Key]
        [Column("cd_tag")]
        public int CdTag { get; set; }
        [Column("nm_tag")]
        [StringLength(250)]
        public string? NmTag { get; set; }

        [ForeignKey("CdTag")]
        [InverseProperty("CdTags")]
        public virtual ICollection<Tarefa> Cds { get; set; }
    }
}
