using GymRoute.DataAccess.Entities.ValueObjects;
using GymRoute.DataAccess.Enums;

namespace GymRoute.DataAccess.Entities;

public abstract class User : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public DateOnly DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public Address Address { get; set; } = null!;
}
