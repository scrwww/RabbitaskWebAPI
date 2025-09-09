using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RabbitaskWebAPI.Models
{
    [Table("tipo_usuario")]
    public partial class TipoUsuario
    {
        public TipoUsuario()
        {
            Usuarios = new HashSet<Usuario>();
        }

        [Key]
        [Column("cd_tipo_usuario")]
        public int CdTipoUsuario { get; set; }
        [Column("nm_tipo_usuario")]
        [StringLength(250)]
        public string? NmTipoUsuario { get; set; }

        [InverseProperty("CdTipoUsuarioNavigation")]
        public virtual ICollection<Usuario> Usuarios { get; set; }
    }
}
