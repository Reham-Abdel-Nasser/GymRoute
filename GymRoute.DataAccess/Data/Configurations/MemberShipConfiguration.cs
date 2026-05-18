using GymRoute.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymRoute.DataAccess.Data.Configurations;

public class MemberShipConfiguration : IEntityTypeConfiguration<MemberShip>
{
    public void Configure(EntityTypeBuilder<MemberShip> builder)
    {
        builder.ToTable(t =>
        {
            t.HasCheckConstraint(
                "CK_MemberShip_DateRange",
                "[EndDate] > [StartDate]");
        });

        //builder.HasIndex(m => new 
        //{ 
        //    m.MemberId, 
        //    m.PlanId
        //}).IsUnique();
    }
}
