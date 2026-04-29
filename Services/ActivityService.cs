using backEnd.Data;
using backEnd.Interfaces;
using backEnd.Models;
using Microsoft.EntityFrameworkCore;

namespace backEnd.Services;

public class ActivityService : IActivityService
{
    private readonly AppDbContext _context;

    public ActivityService(AppDbContext context)
    {
        _context = context;
    }

    public async Task LogActivityAsync(string actionType, string details, string userEmail, string userFullName)
    {
        var activity = new Activity
        {
            ActionType = actionType,
            Details = details,
            UserEmail = userEmail,
            UserFullName = userFullName,
            Timestamp = DateTime.UtcNow
        };

        _context.Activities.Add(activity);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Activity>> GetAllActivitiesAsync()
    {
        return await _context.Activities
            .OrderByDescending(a => a.Timestamp)
            .Take(100)
            .ToListAsync();
    }
}
