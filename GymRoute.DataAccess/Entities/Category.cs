namespace GymRoute.DataAccess.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = null!;

    // TODO : ICollection<Session>
    public ICollection<Session> Sessions { get; set; } = [];
}
