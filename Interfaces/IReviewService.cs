using backEnd.Models;

namespace backEnd.Interfaces;

public interface IReviewService
{
    Task<Review?> AddReviewAsync(Review review);
    Task<IEnumerable<Review>> GetProductReviewsAsync(long productId, string productType);
    Task<double> GetAverageRatingAsync(long productId, string productType);
}
