using GymRoute.DataAccess.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymRoute.DataAccess.Data.Configurations;

public class TrainerConfiguration : UserConfiguration<Trainer>
{
    public override void Configure(EntityTypeBuilder<Trainer> builder)
    {
        base.Configure(builder);

        // Configuration related to Trainer

        builder.Property(t => t.Speciality)
            .HasConversion<string>()
            .HasMaxLength(30);
    }
}
