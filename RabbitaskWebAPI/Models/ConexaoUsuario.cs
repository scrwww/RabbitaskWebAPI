using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RabbitaskWebAPI.Models
{
    /// <summary>
    /// Represents the connection between an Agente and a Usuario Comum
    /// Allows Agentes to manage tasks for their connected usuarios
    /// </summary>
    [Table("conexao_usuario")]
    public partial class ConexaoUsuario
    {
        /// <summary>
        /// ID of the Usuario Comum being managed
        /// </summary>
        [Column("cd_usuario")]
        [Required]
        public int CdUsuario { get; set; }

        /// <summary>
        /// ID of the Agente who manages the usuario
        /// </summary>
        [Column("cd_usuario_agente")]
        [Required]
        public int CdUsuarioAgente { get; set; }

        // Navigation properties

        /// <summary>
        /// Navigation to the Usuario Comum being managed
        /// </summary>
        [ForeignKey("CdUsuario")]
        public virtual Usuario CdUsuarioNavigation { get; set; } = null!;

        /// <summary>
        /// Navigation to the Agente who manages
        /// </summary>
        [ForeignKey("CdUsuarioAgente")]
        public virtual Usuario CdUsuarioAgenteNavigation { get; set; } = null!;
    }
}