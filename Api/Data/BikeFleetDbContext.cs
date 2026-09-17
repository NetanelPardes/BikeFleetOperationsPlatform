using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Data;

public class BikeFleetDbContext: DbContext
{
    public BikeFleetDbContext(DbContextOptions<BikeFleetDbContext> options) :base(options)
    {
        
    }
    public DbSet<Station> Stations => Set<Station>();
    public DbSet<VehicleType> vehicleTypes => Set<VehicleType>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Station>(entity =>
        {
            entity.ToTable("stations");

            entity.HasKey(station => station.StationId);

            entity.Property(station => station.StationId)
                .HasMaxLength(255);

            entity.Property(station => station.AdditionalFieldsJson)
                .HasColumnType("json");
        });

        modelBuilder.Entity<VehicleType>(entity =>
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