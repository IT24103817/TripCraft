using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripCraft.Application.Resources;

namespace TripCraft.Infrastructure.Resources;

public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("vehicles", t =>
        {
            t.HasCheckConstraint("ck_vehicles_seats", "seats > 0");
            t.HasCheckConstraint("ck_vehicles_rate", "rate_per_km_lkr > 0");
        });
        builder.Property(v => v.RegistrationNo).HasMaxLength(20).IsRequired();
        builder.Property(v => v.Type).HasMaxLength(20).IsRequired();
        builder.HasIndex(v => v.RegistrationNo).IsUnique();
    }
}
