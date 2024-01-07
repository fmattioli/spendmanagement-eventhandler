using Domain.Entities;
using Domain.Interfaces;
using MongoDB.Driver;
using Serilog;

namespace Data.Persistence.Repositories
{
    public class ReceiptRepository(IMongoDatabase mongoDb, ILogger _logger) : BaseRepository<Receipt>(mongoDb, "Receipts", _logger), IReceiptRepository
    {
    }
}
