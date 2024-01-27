using Domain.Interfaces;
using Serilog;
using System.Data;

namespace Data.Persistence.UnitOfWork
{
    public class UnitOfWork(IDbTransaction dbTransaction, ILogger logger,
        ISpendManagementEventRepository spendManagementEventRepository) : IUnitOfWork, IDisposable
    {
        public ISpendManagementEventRepository SpendManagementEventRepository { get; } = spendManagementEventRepository;
        private readonly ILogger _logger = logger;
        private readonly IDbTransaction _dbTransaction = dbTransaction;

        public void Commit()
        {
            try
            {
                _dbTransaction.Commit();
                _dbTransaction.Connection?.BeginTransaction();
            }
            catch (Exception exception)
            {
                _dbTransaction.Rollback();
                _logger.Error(exception, "The following error occurred ");
            }
        }

        public void Dispose()
        {
            _dbTransaction.Connection?.Close();
            _dbTransaction.Connection?.Dispose();
            _dbTransaction.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
