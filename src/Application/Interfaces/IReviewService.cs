using LINTelligent.Application.DTOs;
using LINTelligent.Domain;

namespace LINTelligent.Application.Interfaces;

public interface IReviewService
{
    public Task<Guid> SubmitReviewRequestAsync(NewReviewRequest reviewRequest, CancellationToken ct);

    public Task<Review?> GetReviewDetailsAsync(Guid reviewId, CancellationToken ct);
}
