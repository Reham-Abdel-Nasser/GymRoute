using GymRoute.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymRoute.DataAccess.Data.Configurations;

public class HealthRecordConfiguration : IEntityTypeConfiguration<HealthRecord>
{
    public void Configure(EntityTypeBuilder<HealthRecord> builder)
    {
        builder.Property(hr => hr.Height)
            .HasPrecision(5, 2);

        builder.Property(hr => hr.Weight)
            .HasPrecision(5, 2);

        builder.Property(hr => hr.BloodType)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.ToTable(hr =>
        {
            hr.HasCheckConstraint(
                "CK_HealthRecord_Height",
                "[Height] > 0"
                );

            hr.HasCheckConstraint(
                "CK_HealthRecord_Weight",
                "[Weight] > 0"
                );
        });

        builder.HasQueryFilter(hr => !hr.IsDeleted);
    }
}
