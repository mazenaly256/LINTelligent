using LINTelligent.Application.Contracts.Interfaces;
using LINTelligent.Domain;

namespace LINTelligent.Infrastructure.Persistence.Repositories;

public class ReviewRepository(AppDbContext context) : IReviewRepository
{
    public async Task<Guid> AddNewReviewAsync(Review review, CancellationToken ct)
    {
        await context.Reviews.AddAsync(review, ct);
        await context.SaveChangesAsync(ct);

        return review.Id;
    }

    public async Task AddReportToTheReviewAsync(Guid reviewId, string report, CancellationToken ct)
    {
        var reviewFromDB = await this.GetReviewByIdAsync(reviewId, ct);

        if (reviewFromDB is null)
        {
            throw new KeyNotFoundException($"Review with ID: {reviewId} is not found.");
        }

        reviewFromDB.Report = report;

        await context.SaveChangesAsync(ct);
    }

    public async Task ChangeStatusAsync(Guid reviewId, string newStatus, CancellationToken ct)
    {
        Review? reviewFromDB = await context.Reviews.FindAsync(reviewId, ct);

        if (reviewFromDB is null)
        {
            throw new KeyNotFoundException($"Review with ID: {reviewId} is not found.");
        }

        reviewFromDB.Status = newStatus;

        await context.SaveChangesAsync(ct);
    }

    public async Task<Review?> GetReviewByIdAsync(Guid reviewId, CancellationToken ct)
    {
        var reviewFromDB = await context.Reviews.FindAsync(reviewId, ct);

        return reviewFromDB;
    }
}
