using Domain.Entities;
using Domain.Interfaces;
using MongoDB.Driver;
using Serilog;

namespace Data.Persistence.Repositories
{
    public class RecurringReceiptRepository(IMongoDatabase mongoDb, ILogger _logger) : BaseRepository<RecurringReceipt>(mongoDb, "RecurringReceipts", _logger), IRecurringReceiptRepository
    {
    }
}
