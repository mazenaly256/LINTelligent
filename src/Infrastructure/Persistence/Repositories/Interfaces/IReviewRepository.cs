using LINTelligent.Domain;

namespace LINTelligent.Infrastructure.Persistence.Repositories.Interfaces;

public interface IReviewRepository
{
    public void ChangeStatus(Review review, string newStatus);
}
