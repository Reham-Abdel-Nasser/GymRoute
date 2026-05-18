using GymRoute.DataAccess.Enums;

namespace GymRoute.DataAccess.Entities;

public class Trainer : User
{
    public Speciality Speciality { get; set; }
    public DateTime HireDate { get; set; }

    // ICollection<Sessions>
    public ICollection<Session> Sessions { get; set; } = [];
}

