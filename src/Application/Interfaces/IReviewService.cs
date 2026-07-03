using LINTelligent.Entities;

namespace LINTelligent.Application.Interfaces;

public interface IReviewService
{
    public Guid SubmitReviewRequest();

    public Review? GetReviewDetails();
}
