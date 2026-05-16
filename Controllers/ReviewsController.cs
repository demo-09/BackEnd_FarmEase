using backEnd.Interfaces;
using backEnd.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpGet("{productId}/{productType}")]
    public async Task<IActionResult> GetReviews(long productId, string productType)
    {
        var reviews = await _reviewService.GetProductReviewsAsync(productId, productType);
        var avg = await _reviewService.GetAverageRatingAsync(productId, productType);
        return Ok(new { reviews, averageRating = avg });
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AddReview([FromBody] Review review)
    {
        review.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "guest";
        review.UserName = User.FindFirstValue(ClaimTypes.Name) ?? "Guest User";
        review.CreatedAt = DateTime.UtcNow;

        var result = await _reviewService.AddReviewAsync(review);
        if (result == null) return BadRequest("Could not add review.");
        return Ok(result);
    }
}
