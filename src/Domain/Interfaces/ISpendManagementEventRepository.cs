using Domain.Entities;

namespace Domain.Interfaces
{
    public interface ISpendManagementEventRepository
    {
        Task Add(SpendManagementEvent spendManagementEvent);
    }
}
