using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using RabbitaskWebAPI.Models;

namespace RabbitaskWebAPI.Data
{
    public partial class RabbitaskContext : DbContext
    {
        public RabbitaskContext()
        {
        }

        public RabbitaskContext(DbContextOptions<RabbitaskContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Anexo> Anexos { get; set; } = null!;
        public virtual DbSet<Prioridade> Prioridades { get; set; } = null!;
        public virtual DbSet<Tag> Tags { get; set; } = null!;
        public virtual DbSet<Tarefa> Tarefas { get; set; } = null!;
        public virtual DbSet<TarefaFeedback> TarefaFeedbacks { get; set; } = null!;
        public virtual DbSet<TipoSentimento> TipoSentimentos { get; set; } = null!;
        public virtual DbSet<TipoUsuario> TipoUsuarios { get; set; } = null!;
        public virtual DbSet<Topico> Topicos { get; set; } = null!;
        public virtual DbSet<Usuario> Usuarios { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Anexo>(entity =>
            {
                entity.HasKey(e => e.CdAnexo)
                    .HasName("PRIMARY");

                entity.HasOne(d => d.CdTarefaNavigation)
                    .WithMany(p => p.Anexos)
                    .HasForeignKey(d => d.CdTarefa)
                    .HasConstraintName("fk_anexo_tarefa");
            });

            modelBuilder.Entity<Prioridade>(entity =>
            {
                entity.HasKey(e => e.CdPrioridade)
                    .HasName("PRIMARY");
            });

            modelBuilder.Entity<Tag>(entity =>
            {
                entity.HasKey(e => e.CdTag)
                    .HasName("PRIMARY");

                entity.HasMany(d => d.CdTarefas)
                    .WithMany(p => p.CdTags)
                    .UsingEntity<Dictionary<string, object>>(
                        "TarefaTag",
                        l => l.HasOne<Tarefa>().WithMany().HasForeignKey("CdTarefa").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("fk_tarefa_tag_tarefa"),
                        r => r.HasOne<Tag>().WithMany().HasForeignKey("CdTag").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("fk_tarefa_tag_tag"),
                        j =>
                        {
                            j.HasKey("CdTag", "CdTarefa").HasName("PRIMARY");

                            j.ToTable("tarefa_tag");

                            j.HasIndex(new[] { "CdTarefa" }, "fk_tarefa_tag_tarefa");

                            j.IndexerProperty<int>("CdTag").HasColumnName("cd_tag");

                            j.IndexerProperty<int>("CdTarefa").HasColumnName("cd_tarefa");
                        });
            });

            modelBuilder.Entity<Tarefa>(entity =>
            {
                entity.HasKey(e => e.CdTarefa)
                    .HasName("PRIMARY");

                entity.HasOne(d => d.CdPrioridadeNavigation)
                    .WithMany(p => p.Tarefas)
                    .HasForeignKey(d => d.CdPrioridade)
                    .HasConstraintName("fk_tarefa_prioridade");

                entity.HasOne(d => d.CdUsuarioNavigation)
                    .WithMany(p => p.TarefaCdUsuarioNavigations)
                    .HasForeignKey(d => d.CdUsuario)
                    .HasConstraintName("fk_tarefa_usuario");

                entity.HasOne(d => d.CdUsuarioProprietarioNavigation)
                    .WithMany(p => p.TarefaCdUsuarioProprietarioNavigations)
                    .HasForeignKey(d => d.CdUsuarioProprietario)
                    .HasConstraintName("fk_tarefa_usuario_proprietario");
            });

            modelBuilder.Entity<TarefaFeedback>(entity =>
            {
                entity.HasKey(e => new { e.CdTipoSentimento, e.CdTarefa })
                    .HasName("PRIMARY");

                entity.HasOne(d => d.CdTarefaNavigation)
                    .WithMany(p => p.TarefaFeedbacks)
                    .HasForeignKey(d => d.CdTarefa)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_tarefa_feedback_tarefa");

                entity.HasOne(d => d.CdTipoSentimentoNavigation)
                    .WithMany(p => p.TarefaFeedbacks)
                    .HasForeignKey(d => d.CdTipoSentimento)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_tarefa_feedback_tipo_sentimento");
            });

            modelBuilder.Entity<TipoSentimento>(entity =>
            {
                entity.HasKey(e => e.CdTipoSentimento)
                    .HasName("PRIMARY");
            });

            modelBuilder.Entity<TipoUsuario>(entity =>
            {
                entity.HasKey(e => e.CdTipoUsuario)
                    .HasName("PRIMARY");
            });

            modelBuilder.Entity<Topico>(entity =>
            {
                entity.HasKey(e => e.CdTopico)
                    .HasName("PRIMARY");

                entity.HasOne(d => d.CdTarefaNavigation)
                    .WithMany(p => p.Topicos)
                    .HasForeignKey(d => d.CdTarefa)
                    .HasConstraintName("fk_topico_tarefa");
            });

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasKey(e => e.CdUsuario)
                    .HasName("PRIMARY");

                entity.HasOne(d => d.CdTipoUsuarioNavigation)
                    .WithMany(p => p.Usuarios)
                    .HasForeignKey(d => d.CdTipoUsuario)
                    .HasConstraintName("fk_usuario_tipo_usuario");

                entity.HasMany(d => d.CdUsuarioAgentes)
                    .WithMany(p => p.CdUsuarios)
                    .UsingEntity<Dictionary<string, object>>(
                        "ConexaoUsuario",
                        l => l.HasOne<Usuario>().WithMany().HasForeignKey("CdUsuarioAgente").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("fk_conexao_usuario_agente_usuario"),
                        r => r.HasOne<Usuario>().WithMany().HasForeignKey("CdUsuario").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("fk_conexao_usuario_usuario"),
                        j =>
                        {
                            j.HasKey("CdUsuario", "CdUsuarioAgente").HasName("PRIMARY");

                            j.ToTable("conexao_usuario");

                            j.HasIndex(new[] { "CdUsuarioAgente" }, "fk_conexao_usuario_agente_usuario");

                            j.IndexerProperty<int>("CdUsuario").HasColumnName("cd_usuario");

                            j.IndexerProperty<int>("CdUsuarioAgente").HasColumnName("cd_usuario_agente");
                        });

                entity.HasMany(d => d.CdUsuarios)
                    .WithMany(p => p.CdUsuarioAgentes)
                    .UsingEntity<Dictionary<string, object>>(
                        "ConexaoUsuario",
                        l => l.HasOne<Usuario>().WithMany().HasForeignKey("CdUsuario").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("fk_conexao_usuario_usuario"),
                        r => r.HasOne<Usuario>().WithMany().HasForeignKey("CdUsuarioAgente").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("fk_conexao_usuario_agente_usuario"),
                        j =>
                        {
                            j.HasKey("CdUsuario", "CdUsuarioAgente").HasName("PRIMARY");

                            j.ToTable("conexao_usuario");

                            j.HasIndex(new[] { "CdUsuarioAgente" }, "fk_conexao_usuario_agente_usuario");

                            j.IndexerProperty<int>("CdUsuario").HasColumnName("cd_usuario");

                            j.IndexerProperty<int>("CdUsuarioAgente").HasColumnName("cd_usuario_agente");
                        });
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
