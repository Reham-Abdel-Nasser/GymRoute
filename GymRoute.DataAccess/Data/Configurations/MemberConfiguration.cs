using GymRoute.DataAccess.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymRoute.DataAccess.Data.Configurations;

public class MemberConfiguration : UserConfiguration<Member>
{
    public override void Configure(EntityTypeBuilder<Member> builder)
    {
        base.Configure(builder);
        
        // Configuration related to "Member" 
        builder.Property(m => m.Photo)
            .HasMaxLength(500)
            .IsUnicode(false);
    }
}
