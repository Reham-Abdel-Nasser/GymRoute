using GymRoute.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymRoute.DataAccess.Data.Configurations;

public class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.Property(s => s.Description)
            .HasMaxLength(500);

        // 3. Relationships Configurations
        builder.ToTable(t =>
        {
            t.HasCheckConstraint(
                "CK_Session_Capacity",
                "[Capacity] BETWEEN 1 AND 25"
            );

            t.HasCheckConstraint(
                "CK_Session_DateRange",
                "[EndDate] > [StartDate]"
            );
        });

        builder.HasQueryFilter(s => !s.IsDeleted);
    }
}
 