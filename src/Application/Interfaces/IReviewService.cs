using LINTelligent.Application.DTOs;
using LINTelligent.Domain;

namespace LINTelligent.Application.Interfaces;

public interface IReviewService
{
    public Guid SubmitReviewRequest(NewReviewRequest reviewRequest);

    public Review? GetReviewDetails();
}
