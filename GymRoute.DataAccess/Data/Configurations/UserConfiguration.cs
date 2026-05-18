using GymRoute.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymRoute.DataAccess.Data.Configurations;

public class UserConfiguration<T> : IEntityTypeConfiguration<T> where T : User
{
    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        builder.Property(u => u.Name)
            .HasMaxLength(100);

        builder.Property(u => u.Email)
            .HasMaxLength(100);

        builder.Property(u => u.Phone)
            .HasMaxLength(20);

        builder.OwnsOne(u => u.Address, a =>
        {
            a.Property(ad => ad.Street)
                .HasMaxLength(100)
                .HasColumnName("Street");

            a.Property(ad => ad.City)
                .HasMaxLength(100)
                .HasColumnName("City");

            a.Property(ad => ad.BuildingNumber)
                .HasColumnName("BuildingNumber");
        });

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.HasIndex(u => u.Phone)
            .IsUnique();

        // Email format

        // Phone format
        // 010, 011, 012, 015 followed by 8 digits

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_User_Phone",
                "LEN([Phone]) = 11 AND [Phone] LIKE '01[0125]%'");
        });

        // Enum as integer 0, 1
        // User.ToList(); // softdeleted
    }
}
