using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RabbitaskWebAPI.Models
{
    [Table("usuario")]
    [Index("CdTipoUsuario", Name = "fk_usuario_tipo_usuario")]
    [Index("NmEmail", Name = "nm_email", IsUnique = true)]
    public partial class Usuario
    {
        public Usuario()
        {
            TarefaCdUsuarioNavigations = new HashSet<Tarefa>();
            TarefaCdUsuarioProprietarioNavigations = new HashSet<Tarefa>();
            CdUsuarioAgentes = new HashSet<Usuario>();
            CdUsuarios = new HashSet<Usuario>();
        }

        [Key]
        [Column("cd_usuario")]
        public int CdUsuario { get; set; }
        [Column("nm_usuario")]
        [StringLength(64)]
        public string? NmUsuario { get; set; }
        [Column("nm_email")]
        [StringLength(254)]
        public string? NmEmail { get; set; }
        [Column("nm_senha")]
        [StringLength(255)]
        public string? NmSenha { get; set; }
        [Column("cd_telefone")]
        [StringLength(30)]
        public string? CdTelefone { get; set; }
        [Column("cd_tipo_usuario")]
        public int? CdTipoUsuario { get; set; }

        [ForeignKey("CdTipoUsuario")]
        [InverseProperty("Usuarios")]
        public virtual TipoUsuario? CdTipoUsuarioNavigation { get; set; }
        [InverseProperty("CdUsuarioNavigation")]
        public virtual ICollection<Tarefa> TarefaCdUsuarioNavigations { get; set; }
        [InverseProperty("CdUsuarioProprietarioNavigation")]
        public virtual ICollection<Tarefa> TarefaCdUsuarioProprietarioNavigations { get; set; }

        [ForeignKey("CdUsuario")]
        [InverseProperty("CdUsuarios")]
        public virtual ICollection<Usuario> CdUsuarioAgentes { get; set; }
        [ForeignKey("CdUsuarioAgente")]
        [InverseProperty("CdUsuarioAgentes")]
        public virtual ICollection<Usuario> CdUsuarios { get; set; }
    }
}
