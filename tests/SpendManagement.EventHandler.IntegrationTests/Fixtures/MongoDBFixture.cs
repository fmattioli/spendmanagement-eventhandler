using MongoDB.Driver;
using SpendManagement.EventHandler.IntegrationTests.Configuration;

namespace SpendManagement.EventHandler.IntegrationTests.Fixtures
{
    public class MongoDBFixture : IAsyncLifetime
    {
        public readonly IMongoDatabase database;

        private readonly List<Guid> categoryIds = [];
        private readonly List<Guid> receiptIds = [];

        public MongoDBFixture()
        {
            var mongoUrl = new MongoUrl(TestSettings.MongoSettings!.ConnectionString);
            this.database = new MongoClient(mongoUrl).GetDatabase(TestSettings.MongoSettings.Database);
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            if (categoryIds.Count != 0)
            {
                var collection = this.database.GetCollection<Category>("Categories");

                var filter = new FilterDefinitionBuilder<Category>()
                    .In(x => x.Id, categoryIds);

                await collection.DeleteOneAsync(filter);
            }

            if (receiptIds.Count != 0)
            {
                var collection = this.database.GetCollection<Receipt>("Receipts");

                var filter = new FilterDefinitionBuilder<Receipt>()
                    .In(x => x.Id, receiptIds);

                await collection.DeleteOneAsync(filter);
            }
        }

        public void AddCategoryToCleanUp(Guid id)
        {
            this.categoryIds.Add(id);
        }

        public void AddReceiptToCleanUp(Guid id)
        {
            this.receiptIds.Add(id);
        }

        public async Task InsertReceiptAsync(Receipt receipt)
        {
            var collection = this.database.GetCollection<Receipt>("Receipts");
            await collection.InsertOneAsync(receipt);
            AddReceiptToCleanUp(receipt.Id);
        }

        public async Task<Category> FindCategoryAsync(Guid categoryId)
        {
            var collection = this.database.GetCollection<Category>("Categories");
            var category = await collection.FindAsync(category => category.Id == categoryId);
            return category.FirstOrDefault();
        }

        public async Task<Receipt> FindReceiptAsync(Guid categoryId)
        {
            var collection = this.database.GetCollection<Receipt>("Receipts");
            var receipt = await collection.FindAsync(receipt => receipt.Id == categoryId);
            return receipt.FirstOrDefault();
        }

        public async Task InsertCategoryAsync(Category category)
        {
            AddCategoryToCleanUp(category.Id);
            var collection = this.database.GetCollection<Category>("Categories");
            await collection.InsertOneAsync(category);
        }
    }

    public record Category(Guid Id, string? Name, DateTime CreatedDate);

    public record Receipt(Guid Id, Guid CategoryId, string? EstablishmentName, DateTime ReceiptDate, IEnumerable<ReceiptItem>? ReceiptItems, decimal Discount, decimal Total);

    public record ReceiptItem(Guid Id, string ItemName, short Quantity, decimal ItemPrice, decimal TotalPrice, string Observation, decimal ItemDiscount);
}
