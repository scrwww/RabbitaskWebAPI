using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RabbitaskWebAPI.Models
{
    [Table("codigo_conexao")]
    public class CodigoConexao
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [StringLength(8)]
        [Column("codigo")]
        public string Codigo { get; set; } = null!;

        [Required]
        [Column("cd_usuario")]
        public int CdUsuario { get; set; }

        [Required]
        [Column("data_criacao")]
        public DateTime DataCriacao { get; set; }

        [Required]
        [Column("data_expiracao")]
        public DateTime DataExpiracao { get; set; }

        [Required]
        [Column("usado")]
        public bool Usado { get; set; }

        // Navegação para Usuario
        [ForeignKey("CdUsuario")]
        public virtual Usuario? Usuario { get; set; }
    }
}