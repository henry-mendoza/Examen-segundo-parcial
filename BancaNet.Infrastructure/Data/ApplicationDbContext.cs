using BancaNet.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BancaNet.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }


    public DbSet<Cliente> Clientes {  get; set; }
    public DbSet<Empleado> Empleados {  get; set; }
    public DbSet<Sucursal> Sucursales {  get; set; }
    public DbSet<Prestamo> Prestamos {  get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<DetallePrestamo> DetallesPrestamos {  get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);



        // Cliente
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.IdCliente);
            entity.Property(e => e.IdCliente).ValueGeneratedOnAdd();
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Direccion).IsRequired().HasMaxLength(250);
            entity.Property(e => e.Telefono).IsRequired().HasMaxLength(20);

            entity.HasOne(e => e.Usuario)
                  .WithOne(u => u.Cliente)
                  .HasForeignKey<Cliente>(e => e.IdUsuario)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ApplicationUser>(static entity =>
        {
            entity.Property(e => e.NombreCompleto)
            .IsRequired()
            .HasMaxLength(75);
        });

        // Empleado
        modelBuilder.Entity<Empleado>(entity =>
        {
            entity.HasKey(e => e.IdEmpleado);
            entity.Property(e => e.IdEmpleado).ValueGeneratedOnAdd();
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Cargo).IsRequired().HasMaxLength(100);

            entity.HasOne(e => e.Usuario)
                  .WithOne(u => u.Empleado)
                  .HasForeignKey<Empleado>(e => e.IdUsuario)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Sucursal)
                  .WithMany(s => s.Empleados)
                  .HasForeignKey(e => e.IdSucursal)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Sucursal
        modelBuilder.Entity<Sucursal>(entity =>
        {
            entity.HasKey(e => e.IdSucursal);
            entity.Property(e => e.IdSucursal).ValueGeneratedOnAdd();
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Direccion).IsRequired().HasMaxLength(250);
        });

        // DetallePrestamo
        modelBuilder.Entity<DetallePrestamo>(entity =>
        {
            entity.HasKey(e => e.IdDetalle);
            entity.Property(e => e.IdDetalle).ValueGeneratedOnAdd();
            entity.Property(e => e.SaldoInicial).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Interes).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Abono).HasColumnType("decimal(18,2)");
            entity.Property(e => e.FechaAbono).HasColumnType("date");
            entity.Property(e => e.Mora).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Total).HasColumnType("decimal(18,2)");
        });

        // Prestamo
        modelBuilder.Entity<Prestamo>(entity =>
        {
            entity.HasKey(e => e.IdPrestamo);
            entity.Property(e => e.IdPrestamo).ValueGeneratedOnAdd();
            entity.Property(e => e.Monto).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TasaInteres).HasColumnType("decimal(5,2)");
            entity.Property(e => e.FechaVencimientoActual).HasColumnType("date");
            entity.Property(e => e.TasaMoraDiaria).HasColumnType("decimal(5,2)");
            entity.Property(e => e.DiasGracia).HasColumnType("integer");

            entity.HasOne(e => e.Cliente)
                  .WithMany(c => c.Prestamos)
                  .HasForeignKey(e => e.IdCliente)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Empleado)
                  .WithMany(em => em.PrestamosGestionados)
                  .HasForeignKey(e => e.IdEmpleado)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.DetallePrestamo)
                  .WithMany(d => d.Prestamos)
                  .HasForeignKey(e => e.IdDetalle)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
