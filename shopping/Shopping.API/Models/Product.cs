using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Shopping.API.Models
{
    public class Product
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("Name")]
        public string Name { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        // 產品描述（選填）
        public string? Description { get; set; } = string.Empty;

        // 產品圖片檔名（選填）
        public string? ImageFile { get; set; } = string.Empty;

        public decimal Price { get; set; }
    }
}