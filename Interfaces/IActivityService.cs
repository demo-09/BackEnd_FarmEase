using backEnd.Models;

namespace backEnd.Interfaces;

public interface IActivityService
{
    Task LogActivityAsync(string actionType, string details, string userEmail, string userFullName);
    Task<IEnumerable<Activity>> GetAllActivitiesAsync();
}
