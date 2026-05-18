using GymRoute.DataAccess.Enums;

namespace GymRoute.DataAccess.Entities;

public class HealthRecord : BaseEntity
{
    public decimal Height { get; set; }
    public decimal Weight { get; set; }

    public BloodType BloodType { get; set; }

    public int MwmberId { get; set; }

    public Member Member { get; set; } = null!;
}
