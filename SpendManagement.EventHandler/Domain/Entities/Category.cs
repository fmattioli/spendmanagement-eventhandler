using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities
{
    public class Category
    {
        public Category(Guid id, string name, DateTime createdDate)
        {
            Id = id;
            Name = name;
            CreatedDate = createdDate;
        }

        [BsonId]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
