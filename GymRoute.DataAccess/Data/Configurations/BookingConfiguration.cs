using GymRoute.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymRoute.DataAccess.Data.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.Property(b => b.IsAttended)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasOne(b => b.Member)
            .WithMany(m => m.Bookings)
            .HasForeignKey(b => b.MemberId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(b => new 
        { 
            b.MemberId, 
            b.SessionId 
        }).IsUnique();

        builder.ToTable(t =>
        {
            t.HasCheckConstraint(
                "CK_Booking_Date", 
                "[Date] >= GETDATE()"
            );
        });
    }
}
