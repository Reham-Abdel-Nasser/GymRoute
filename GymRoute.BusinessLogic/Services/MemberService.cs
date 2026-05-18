using GymRoute.BusinessLogic.Interfaces;
using GymRoute.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymRoute.BusinessLogic.Services;

public class MemberService(IGymDbContext db)
{
    public async Task<IReadOnlyList<Member>> GetMembersWithBookingsAsync(
        CancellationToken cancellationToken = default)
        => await db.Users
            .OfType<Member>()
            .Include(m => m.Bookings)
            .OrderBy(m => m.Name)
            .ToListAsync(cancellationToken);

    public async Task RegisterMemberAsync(Member member, CancellationToken cancellationToken = default)
    {
        db.Users.Add(member);
        await db.SaveChangesAsync(cancellationToken);
    }
}
