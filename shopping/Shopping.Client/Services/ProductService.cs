using Shopping.Client.Models;
using Shopping.Client.Settings;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text;

namespace Shopping.Client.Services
{
    public class ProductService : IProductService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ProductService> _logger;
        private readonly string _baseUrl;

        public ProductService(
            HttpClient httpClient, 
            IOptions<ApiSettings> settings,
            ILogger<ProductService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _baseUrl = settings.Value.BaseUrl;
        }

        public async Task<IEnumerable<Product>> GetProducts()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/v1/Product");
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                var products = JsonSerializer.Deserialize<List<Product>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                return products ?? new List<Product>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得產品列表時發生錯誤");
                // 如果 API 失敗，返回空列表或使用本地資料
                return new List<Product>();
            }
        }

        public async Task<Product> GetProduct(string id)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/v1/Product/{id}");
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            var product = JsonSerializer.Deserialize<Product>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            return product!;
        }

        public async Task<Product> CreateProduct(Product product)
        {
            var json = JsonSerializer.Serialize(product);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync($"{_baseUrl}/api/v1/Product", data);
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            var createdProduct = JsonSerializer.Deserialize<Product>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            return createdProduct!;
        }

        public async Task<Product> UpdateProduct(Product product)
        {
            var json = JsonSerializer.Serialize(product);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PutAsync($"{_baseUrl}/api/v1/Product", data);
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            var updatedProduct = JsonSerializer.Deserialize<Product>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            return updatedProduct!;
        }

        public async Task DeleteProduct(string id)
        {
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/api/v1/Product/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}