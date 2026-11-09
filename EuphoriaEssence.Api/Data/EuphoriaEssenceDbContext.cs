using System;
using System.Collections.Generic;
using EuphoriaEssence.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EuphoriaEssence.Api.Data;

public partial class EuphoriaEssenceDbContext : DbContext
{
    public EuphoriaEssenceDbContext(DbContextOptions<EuphoriaEssenceDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Carritos> Carritos { get; set; }

    public virtual DbSet<Categorias> Categorias { get; set; }

    public virtual DbSet<DetalleCarrito> DetalleCarrito { get; set; }

    public virtual DbSet<DetallePedido> DetallePedido { get; set; }

    public virtual DbSet<EstadosPedido> EstadosPedido { get; set; }

    public virtual DbSet<ImagenesProducto> ImagenesProducto { get; set; }

    public virtual DbSet<Marcas> Marcas { get; set; }

    public virtual DbSet<Pedidos> Pedidos { get; set; }

    public virtual DbSet<Productos> Productos { get; set; }

    public virtual DbSet<Roles> Roles { get; set; }

    public virtual DbSet<Usuarios> Usuarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Carritos>(entity =>
        {
            entity.HasKey(e => e.IdCarrito);

            entity.HasIndex(e => e.IdUsuario, "IX_Carritos_IdUsuario");

            entity.HasIndex(e => e.IdUsuario, "UX_Carritos_UsuarioActivo")
                .IsUnique()
                .HasFilter("([Activo]=(1))");

            entity.Property(e => e.Activo).HasDefaultValue(true, "DF_Carritos_Activo");
            entity.Property(e => e.FechaCreacion)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())", "DF_Carritos_FechaCreacion");

            entity.HasOne(d => d.IdUsuarioNavigation)
                .WithMany(p => p.Carritos)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Carritos_Usuarios");
        });

        modelBuilder.Entity<Categorias>(entity =>
        {
            entity.HasKey(e => e.IdCategoria);

            entity.HasIndex(e => e.NombreCategoria, "UQ_Categorias_NombreCategoria").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true, "DF_Categorias_Activo");
            entity.Property(e => e.NombreCategoria).HasMaxLength(100);
        });

        modelBuilder.Entity<DetalleCarrito>(entity =>
        {
            entity.HasKey(e => e.IdDetalleCarrito);

            entity.HasIndex(e => e.IdProducto, "IX_DetalleCarrito_IdProducto");

            entity.HasIndex(e => new { e.IdCarrito, e.IdProducto }, "UQ_DetalleCarrito_Producto").IsUnique();

            entity.HasOne(d => d.IdCarritoNavigation).WithMany(p => p.DetalleCarrito)
                .HasForeignKey(d => d.IdCarrito)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetalleCarrito_Carritos");

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.DetalleCarrito)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetalleCarrito_Productos");
        });

        modelBuilder.Entity<DetallePedido>(entity =>
        {
            entity.HasKey(e => e.IdDetallePedido);

            entity.HasIndex(e => e.IdProducto, "IX_DetallePedido_IdProducto");

            entity.HasIndex(e => new { e.IdPedido, e.IdProducto }, "UQ_DetallePedido_Producto").IsUnique();

            entity.Property(e => e.PrecioUnitario).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Subtotal)
                .HasComputedColumnSql("(CONVERT([decimal](28,2),[Cantidad]*[PrecioUnitario]))", false)
                .HasColumnType("decimal(28, 2)");

            entity.HasOne(d => d.IdPedidoNavigation).WithMany(p => p.DetallePedido)
                .HasForeignKey(d => d.IdPedido)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetallePedido_Pedidos");

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.DetallePedido)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetallePedido_Productos");
        });

        modelBuilder.Entity<EstadosPedido>(entity =>
        {
            entity.HasKey(e => e.IdEstadoPedido);

            entity.HasIndex(e => e.NombreEstado, "UQ_EstadosPedido_NombreEstado").IsUnique();

            entity.Property(e => e.NombreEstado).HasMaxLength(100);
        });

        modelBuilder.Entity<ImagenesProducto>(entity =>
        {
            entity.HasKey(e => e.IdImagen);

            entity.HasIndex(e => e.IdProducto, "IX_ImagenesProducto_IdProducto");

            entity.HasIndex(e => e.IdProducto, "UX_ImagenesProducto_Principal")
                .IsUnique()
                .HasFilter("([EsPrincipal]=(1))");

            entity.Property(e => e.UrlImagen).HasMaxLength(500);

            entity.HasOne(d => d.IdProductoNavigation)
                .WithMany(p => p.ImagenesProducto)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ImagenesProducto_Productos");
        });

        modelBuilder.Entity<Marcas>(entity =>
        {
            entity.HasKey(e => e.IdMarca);

            entity.HasIndex(e => e.NombreMarca, "UQ_Marcas_NombreMarca").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true, "DF_Marcas_Activo");
            entity.Property(e => e.NombreMarca).HasMaxLength(100);
        });

        modelBuilder.Entity<Pedidos>(entity =>
        {
            entity.HasKey(e => e.IdPedido);

            entity.HasIndex(e => e.IdEstadoPedido, "IX_Pedidos_IdEstadoPedido");

            entity.HasIndex(e => new { e.IdUsuario, e.FechaPedido }, "IX_Pedidos_Usuario_Fecha").IsDescending(false, true);

            entity.Property(e => e.DireccionEnvio).HasMaxLength(200);
            entity.Property(e => e.FechaPedido)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())", "DF_Pedidos_FechaPedido");

            entity.HasOne(d => d.IdEstadoPedidoNavigation).WithMany(p => p.Pedidos)
                .HasForeignKey(d => d.IdEstadoPedido)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Pedidos_EstadosPedido");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Pedidos)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Pedidos_Usuarios");
        });

        modelBuilder.Entity<Productos>(entity =>
        {
            entity.HasKey(e => e.IdProducto);

            entity.HasIndex(e => e.IdCategoria, "IX_Productos_IdCategoria");

            entity.HasIndex(e => e.IdMarca, "IX_Productos_IdMarca");

            entity.Property(e => e.Activo).HasDefaultValue(true, "DF_Productos_Activo");
            entity.Property(e => e.Concentracion).HasMaxLength(50);
            entity.Property(e => e.Descripcion).HasMaxLength(700);
            entity.Property(e => e.Nombre).HasMaxLength(150);
            entity.Property(e => e.Precio).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.IdCategoriaNavigation).WithMany(p => p.Productos)
                .HasForeignKey(d => d.IdCategoria)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Productos_Categorias");

            entity.HasOne(d => d.IdMarcaNavigation).WithMany(p => p.Productos)
                .HasForeignKey(d => d.IdMarca)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Productos_Marcas");
        });

        modelBuilder.Entity<Roles>(entity =>
        {
            entity.HasKey(e => e.IdRol);

            entity.HasIndex(e => e.NombreRol, "UQ_Roles_NombreRol").IsUnique();

            entity.Property(e => e.NombreRol).HasMaxLength(50);
        });

        modelBuilder.Entity<Usuarios>(entity =>
        {
            entity.HasKey(e => e.IdUsuario);

            entity.HasIndex(e => e.IdRol, "IX_Usuarios_IdRol");

            entity.HasIndex(e => e.Email, "UQ_Usuarios_Email").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true, "DF_Usuarios_Activo");
            entity.Property(e => e.Apellido).HasMaxLength(100);
            entity.Property(e => e.Direccion).HasMaxLength(200);
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .UseCollation("Latin1_General_100_CI_AS");
            entity.Property(e => e.Nombre).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(500);
            entity.Property(e => e.Telefono).HasMaxLength(30);

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdRol)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Usuarios_Roles");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
