namespace GymRoute.DataAccess.Entities.ValueObjects;

public class Address
{
    public string City { get; set; } = null!;
    public string Street { get; set; } = null!;
    public int BuildingNumber { get; set; }
}
