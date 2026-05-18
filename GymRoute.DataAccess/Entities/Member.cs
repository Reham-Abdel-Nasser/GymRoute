namespace GymRoute.DataAccess.Entities;

public class Member : User
{
    public string? Photo { get; set; }
    public DateTime JoinDate { get; set; }
    public int HealthRecordId { get; set; }
    // HealthRecord
    public HealthRecord HealthRecords { get; set; } = null!;

    // ICollection<Bookings>
    public ICollection<Booking> Bookings { get; set; } = [];

    // ICollection<MemberShips>
    public ICollection<MemberShip> MemberShips { get; set; } = [];
}
