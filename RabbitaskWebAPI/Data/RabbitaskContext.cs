﻿using System;
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
        public virtual DbSet<ConexaoUsuario> ConexaoUsuarios { get; set; } = null!;
        public virtual DbSet<CodigoConexao> CodigosConexao { get; set; } = null!; // ADICIONADO

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Anexo>(entity =>
            {
                entity.HasKey(e => new { e.CdAnexo, e.CdUsuario, e.CdTarefa })
                    .HasName("PRIMARY");

                entity.HasOne(d => d.Cd)
                    .WithMany(p => p.Anexos)
                    .HasForeignKey(d => new { d.CdUsuario, d.CdTarefa })
                    .OnDelete(DeleteBehavior.ClientSetNull)
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

                entity.HasMany(d => d.Cds)
                    .WithMany(p => p.CdTags)
                    .UsingEntity<Dictionary<string, object>>(
                        "TarefaTag",
                        l => l.HasOne<Tarefa>().WithMany().HasForeignKey("CdUsuario", "CdTarefa").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("fk_tarefa_tag_tarefa"),
                        r => r.HasOne<Tag>().WithMany().HasForeignKey("CdTag").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("fk_tarefa_tag_tag"),
                        j =>
                        {
                            j.HasKey("CdTag", "CdUsuario", "CdTarefa").HasName("PRIMARY");

                            j.ToTable("tarefa_tag");

                            j.HasIndex(new[] { "CdUsuario", "CdTarefa" }, "fk_tarefa_tag_tarefa");

                            j.IndexerProperty<int>("CdTag").HasColumnName("cd_tag");

                            j.IndexerProperty<int>("CdUsuario").HasColumnName("cd_usuario");

                            j.IndexerProperty<int>("CdTarefa").HasColumnName("cd_tarefa");
                        });
            });

            modelBuilder.Entity<Tarefa>(entity =>
            {
                entity.HasKey(e => new { e.CdUsuario, e.CdTarefa })
                    .HasName("PRIMARY");

                entity.HasOne(d => d.CdPrioridadeNavigation)
                    .WithMany(p => p.Tarefas)
                    .HasForeignKey(d => d.CdPrioridade)
                    .HasConstraintName("fk_tarefa_prioridade");

                entity.HasOne(d => d.CdUsuarioNavigation)
                    .WithMany(p => p.TarefaCdUsuarioNavigations)
                    .HasForeignKey(d => d.CdUsuario)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_tarefa_usuario");

                entity.HasOne(d => d.CdUsuarioProprietarioNavigation)
                    .WithMany(p => p.TarefaCdUsuarioProprietarioNavigations)
                    .HasForeignKey(d => d.CdUsuarioProprietario)
                    .HasConstraintName("fk_tarefa_usuario_proprietario");
            });

            modelBuilder.Entity<TarefaFeedback>(entity =>
            {
                entity.HasKey(e => new { e.CdTipoSentimento, e.CdUsuario, e.CdTarefa })
                    .HasName("PRIMARY");

                entity.HasOne(d => d.CdTipoSentimentoNavigation)
                    .WithMany(p => p.TarefaFeedbacks)
                    .HasForeignKey(d => d.CdTipoSentimento)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_tarefa_feedback_tipo_sentimento");

                entity.HasOne(d => d.Cd)
                    .WithMany(p => p.TarefaFeedbacks)
                    .HasForeignKey(d => new { d.CdUsuario, d.CdTarefa })
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_tarefa_feedback_tarefa");
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
                entity.HasKey(e => new { e.CdTopico, e.CdUsuario, e.CdTarefa })
                    .HasName("PRIMARY");

                entity.HasOne(d => d.Cd)
                    .WithMany(p => p.Topicos)
                    .HasForeignKey(d => new { d.CdUsuario, d.CdTarefa })
                    .OnDelete(DeleteBehavior.ClientSetNull)
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
            });

            modelBuilder.Entity<ConexaoUsuario>(entity =>
            {
                entity.HasKey(e => new { e.CdUsuario, e.CdUsuarioAgente })
                    .HasName("PRIMARY");

                entity.ToTable("conexao_usuario");

                entity.Property(e => e.CdUsuario)
                    .HasColumnName("cd_usuario")
                    .IsRequired();

                entity.Property(e => e.CdUsuarioAgente)
                    .HasColumnName("cd_usuario_agente")
                    .IsRequired();

                // Foreign key to Usuario (Usuario Comum sendo gerido)
                entity.HasOne(d => d.CdUsuarioNavigation)
                    .WithMany()
                    .HasForeignKey(d => d.CdUsuario)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_conexao_usuario_usuario");

                // Foreign key to Usuario (Agente gerindo)
                entity.HasOne(d => d.CdUsuarioAgenteNavigation)
                    .WithMany()
                    .HasForeignKey(d => d.CdUsuarioAgente)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_conexao_usuario_agente_usuario");

                entity.HasIndex(e => e.CdUsuarioAgente)
                    .HasDatabaseName("fk_conexao_usuario_agente_usuario");
            });

            // ====== CONFIGURAÇÃO DA NOVA ENTIDADE CodigoConexao ======
            modelBuilder.Entity<CodigoConexao>(entity =>
            {
                entity.HasKey(e => e.Id)
                    .HasName("PRIMARY");

                entity.ToTable("codigo_conexao");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Codigo)
                    .HasColumnName("codigo")
                    .HasMaxLength(8)
                    .IsRequired();

                entity.Property(e => e.CdUsuario)
                    .HasColumnName("cd_usuario")
                    .IsRequired();

                entity.Property(e => e.DataCriacao)
                    .HasColumnName("data_criacao")
                    .HasColumnType("datetime")
                    .IsRequired();

                entity.Property(e => e.DataExpiracao)
                    .HasColumnName("data_expiracao")
                    .HasColumnType("datetime")
                    .IsRequired();

                entity.Property(e => e.Usado)
                    .HasColumnName("usado")
                    .HasDefaultValue(false)
                    .IsRequired();

                // Índice para busca rápida por código
                entity.HasIndex(e => e.Codigo)
                    .HasDatabaseName("idx_codigo_conexao_codigo");

                // Índice para busca por usuário
                entity.HasIndex(e => e.CdUsuario)
                    .HasDatabaseName("idx_codigo_conexao_usuario");

                // Foreign key para Usuario
                entity.HasOne(d => d.Usuario)
                    .WithMany()
                    .HasForeignKey(d => d.CdUsuario)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("fk_codigo_conexao_usuario");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}