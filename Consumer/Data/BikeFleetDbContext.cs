using Consumer.Entities;
using Microsoft.EntityFrameworkCore;

namespace Consumer.Data;

public class BikeFleetDbContext : DbContext
{
    public BikeFleetDbContext(DbContextOptions<BikeFleetDbContext> options) : base(options)
    {
    }

    public DbSet<StationEntity> Stations => Set<StationEntity>();

    public DbSet<VehicleTypeEntity> VehicleTypes => Set<VehicleTypeEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StationEntity>(entity =>
        {
            entity.ToTable("stations");

            entity.HasKey(station => station.StationId);

            entity.Property(station => station.StationId)
                .HasMaxLength(255);

            entity.Property(station => station.AdditionalFieldsJson)
                .HasColumnType("json");
        });

        modelBuilder.Entity<VehicleTypeEntity>(entity =>
        {
            entity.ToTable("vehicle_types");

            entity.HasKey(vehicleType => vehicleType.VehicleTypeId);

            entity.Property(vehicleType => vehicleType.VehicleTypeId)
                .HasMaxLength(255);

            entity.Property(vehicleType => vehicleType.FormFactor)
                .HasMaxLength(100);

            entity.Property(vehicleType => vehicleType.PropulsionType)
                .HasMaxLength(100);

            entity.Property(vehicleType => vehicleType.AdditionalFieldsJson)
                .HasColumnType("json");
        });
    }
}