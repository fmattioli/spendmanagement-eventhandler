using Domain.Interfaces;

namespace Data.Persistence.UnitOfWork
{
    public interface IUnitOfWork
    {
        ISpendManagementEventRepository SpendManagementEventRepository { get; }

        void Commit();
        void Dispose();
    }
}
