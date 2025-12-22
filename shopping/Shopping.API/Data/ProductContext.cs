using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Shopping.API.Models;
using Shopping.API.Settings;

namespace Shopping.API.Data
{
    public class ProductContext : IProductContext
    {
        public ProductContext(IOptions<DatabaseSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            var database = client.GetDatabase(settings.Value.DatabaseName);
            Products = database.GetCollection<Product>(settings.Value.CollectionName);
            
            // 初始化種子資料
            ProductContextSeed.SeedData(Products);
        }

        public IMongoCollection<Product> Products { get; }
    }
}