using MongoDB.Driver;
using Shopping.API.Models;

namespace Shopping.API.Data
{
    public interface IProductContext
    {
        IMongoCollection<Product> Products { get; }
    }
}