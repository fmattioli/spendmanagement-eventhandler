using Domain.Entities;
using Domain.Interfaces;
using MongoDB.Driver;

namespace Data.Persistence.Repositories
{
    public class ReceiptRepository : BaseRepository<Receipt>, IReceiptRepository
    {
        public ReceiptRepository(IMongoDatabase mongoDb) : base(mongoDb, "Receipts")
        {
        }
    }
}
