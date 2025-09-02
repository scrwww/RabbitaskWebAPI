using Microsoft.EntityFrameworkCore;
using RabbitaskWebAPI.models;

namespace RabbitaskWebAPI.contexts
{
    public class RabbitaskContext : DbContext
    {
        public RabbitaskContext(DbContextOptions<RabbitaskContext> options) : base(options) { }

        // DbSets = Tables
        public DbSet<TipoUsuario> TipoUsuarios { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        //public DbSet<ConexaoUsuario> ConexoesUsuarios { get; set; }
        //public DbSet<Tag> Tags { get; set; }
        //public DbSet<TipoSentimento> TiposSentimento { get; set; }
        //public DbSet<Prioridade> Prioridades { get; set; }
        //public DbSet<Tarefa> Tarefas { get; set; }
        //public DbSet<TarefaTag> TarefasTags { get; set; }
        //public DbSet<TarefaFeedback> TarefasFeedback { get; set; }
        //public DbSet<Topico> Topicos { get; set; }
        //public DbSet<Anexo> Anexos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ConexaoUsuario>()
                .HasKey(cu => new { cu.CdUsuario, cu.CdUsuarioAgente });

            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.TipoUsuario)
                .WithMany()
                .HasForeignKey(u => u.CdTipoUsuario);


            base.OnModelCreating(modelBuilder);
        }
    }
}
}
