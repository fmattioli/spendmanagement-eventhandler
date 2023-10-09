using Domain.Entities;
using Domain.Interfaces;
using MongoDB.Driver;
using Serilog;

namespace Data.Persistence.Repositories
{
    public class ReceiptRepository : BaseRepository<Receipt>, IReceiptRepository
    {
        public ReceiptRepository(IMongoDatabase mongoDb, ILogger _logger) : base(mongoDb, "Receipts", _logger)
        {
        }
    }
}
