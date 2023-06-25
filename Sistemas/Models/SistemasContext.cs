using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Sistemas.Models;

public partial class SistemasContext : DbContext
{
    public SistemasContext()
    {
    }

    public SistemasContext(DbContextOptions<SistemasContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Bitacoras> Bitacoras { get; set; }

    public virtual DbSet<Roles> Roles { get; set; }

    public virtual DbSet<Usuarios> Usuarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=localhost;user=root;password=root;database=Sistemas", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.28-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Bitacoras>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("bitacoras");

            entity.HasIndex(e => e.IdUsuario, "fkIBUsuario");

            entity.Property(e => e.Correo).HasMaxLength(90);
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.Observaciones).HasColumnType("text");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Bitacoras)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fkIBUsuario");
        });

        modelBuilder.Entity<Roles>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("roles");

            entity.Property(e => e.Nombre).HasMaxLength(30);
        });

        modelBuilder.Entity<Usuarios>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("usuarios");

            entity.HasIndex(e => e.Correo, "Correo_UNIQUE").IsUnique();

            entity.HasIndex(e => e.IdRol, "fk_IdRolDeUsuario");

            entity.Property(e => e.Contrasena).HasMaxLength(256);
            entity.Property(e => e.Correo).HasMaxLength(90);
            entity.Property(e => e.Nombre).HasMaxLength(80);

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdRol)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_IdRolDeUsuario");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
