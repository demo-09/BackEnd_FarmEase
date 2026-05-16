using backEnd.Data;
using backEnd.Interfaces;
using backEnd.Models;
using Microsoft.EntityFrameworkCore;

namespace backEnd.Services;

public class ReviewService : IReviewService
{
    private readonly AppDbContext _context;

    public ReviewService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Review?> AddReviewAsync(Review review)
    {
        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();
        return review;
    }

    public async Task<IEnumerable<Review>> GetProductReviewsAsync(long productId, string productType)
    {
        return await _context.Reviews
            .Where(r => r.ProductId == productId && r.ProductType.ToLower() == productType.ToLower())
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<double> GetAverageRatingAsync(long productId, string productType)
    {
        var reviews = await _context.Reviews
            .Where(r => r.ProductId == productId && r.ProductType.ToLower() == productType.ToLower())
            .ToListAsync();

        if (!reviews.Any()) return 0;
        return reviews.Average(r => r.Rating);
    }
}
