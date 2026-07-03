using LINTelligent.Entities;
using LINTelligent.Infrastructure.Persistence.Repositories.Interfaces;

namespace LINTelligent.Infrastructure.Persistence.Repositories.Implementations;

public class ReviewRepository : IReviewRepository
{
    public void ChangeStatus(Review review, string newStatus)
    {
        throw new NotImplementedException();
    }
}
