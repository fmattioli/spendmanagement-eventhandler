using Domain.Entities;

namespace Domain.Interfaces
{
    public interface ISpendManagementEventRepository
    {
        Task<Guid> Add(SpendManagementEvent spendManagementEvent);
    }
}
