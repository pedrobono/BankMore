using BankMore.ContaCorrente.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BankMore.ContaCorrente.Infrastructure.Persistence {
    public class DataBaseContext : DbContext {
        public DbSet<Conta> ContaCorrente { get; set; }
        public DbSet<Movimento> Movimento { get; set; }
        public DbSet<Idempotencia> Idempotencia { get; set; }

        public DataBaseContext(DbContextOptions<DataBaseContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Conta>(entity => {
                entity.ToTable("contacorrente");
                entity.HasKey(e => e.IdContaCorrente);
                entity.Property(e => e.IdContaCorrente).HasColumnName("idcontacorrente");
                entity.Property(e => e.Numero).HasColumnName("numero").IsRequired();
                entity.Property(e => e.Nome).HasColumnName("nome").HasMaxLength(100).IsRequired();
                entity.Property(e => e.Ativo).HasColumnName("ativo").IsRequired().HasDefaultValue(0);
                entity.Property(e => e.Senha).HasColumnName("senha").HasMaxLength(100).IsRequired();
                entity.Property(e => e.Salt).HasColumnName("salt").HasMaxLength(100).IsRequired();
                entity.HasIndex(e => e.Numero).IsUnique();
            });

            modelBuilder.Entity<Movimento>(entity => {
                entity.ToTable("movimento");
                entity.HasKey(e => e.IdMovimento);
                entity.Property(e => e.IdMovimento).HasColumnName("idmovimento");
                entity.Property(e => e.IdContaCorrente).HasColumnName("idcontacorrente").IsRequired();
                entity.Property(e => e.DataMovimento).HasColumnName("datamovimento").HasMaxLength(25).IsRequired();
                entity.Property(e => e.TipoMovimento).HasColumnName("tipomovimento").HasMaxLength(1).IsRequired();
                entity.Property(e => e.Valor).HasColumnName("valor").HasColumnType("REAL").IsRequired();
                entity.HasOne(e => e.Conta)
                    .WithMany()
                    .HasForeignKey(e => e.IdContaCorrente)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Idempotencia>(entity => {
                entity.ToTable("idempotencia");
                entity.HasKey(e => e.ChaveIdempotencia);
                entity.Property(e => e.ChaveIdempotencia).HasColumnName("chave_idempotencia");
                entity.Property(e => e.Requisicao).HasColumnName("requisicao").HasMaxLength(1000);
                entity.Property(e => e.Resultado).HasColumnName("resultado").HasMaxLength(1000);
            });
        }
    }
}