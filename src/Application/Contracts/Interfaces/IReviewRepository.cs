using LINTelligent.Domain;

namespace LINTelligent.Application.Contracts.Interfaces;

public interface IReviewRepository
{
    public Task<Guid> AddNewReviewAsync(Review review, CancellationToken ct);

    public Task ChangeStatusAsync(Guid reviewId, string newStatus, CancellationToken ct);

    public Task AddReportToTheReviewAsync(Guid reviewId, string report, CancellationToken ct);

    public Task<Review?> GetReviewByIdAsync(Guid reviewId, CancellationToken ct);
}
