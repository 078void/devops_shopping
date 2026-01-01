using Azure.Data.Tables;
using Microsoft.Extensions.Options;
using Shopping.API.Models;
using Shopping.API.Settings;

namespace Shopping.API.Services
{
    /// <summary>
    /// 訂閱服務實作 - 使用 Azure Table Storage
    /// </summary>
    public class SubscriptionService : ISubscriptionService
    {
        private readonly TableClient _tableClient;
        private readonly ILogger<SubscriptionService> _logger;

        public SubscriptionService(
            IOptions<AzureStorageSettings> settings,
            ILogger<SubscriptionService> logger)
        {
            _logger = logger;
            
            // 建立 Table Client
            _tableClient = new TableClient(
                settings.Value.ConnectionString,
                "ProductSubscriptions");
            
            _tableClient.CreateIfNotExists();
        }

        public async Task<bool> SubscribeAsync(ProductSubscription subscription)
        {
            try
            {
                // 使用 ProductId + Email 作為唯一識別
                var entity = new TableEntity(
                    partitionKey: subscription.ProductId,
                    rowKey: subscription.Email)
                {
                    { "ProductName", subscription.ProductName },
                    { "Email", subscription.Email },
                    { "NotifyOnPriceIncrease", subscription.NotifyOnPriceIncrease },
                    { "NotifyOnPriceDecrease", subscription.NotifyOnPriceDecrease },
                    { "SubscribedAt", subscription.SubscribedAt }
                };

                await _tableClient.UpsertEntityAsync(entity);
                
                _logger.LogInformation($"✅ {subscription.Email} 已訂閱 {subscription.ProductName}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ 訂閱失敗: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UnsubscribeAsync(string email, string productId)
        {
            try
            {
                await _tableClient.DeleteEntityAsync(productId, email);
                _logger.LogInformation($"✅ {email} 已取消訂閱 {productId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ 取消訂閱失敗: {ex.Message}");
                return false;
            }
        }

        public async Task<List<ProductSubscription>> GetSubscribersAsync(string productId)
        {
            try
            {
                var subscriptions = new List<ProductSubscription>();
                
                // 查詢該商品的所有訂閱者
                var query = _tableClient.QueryAsync<TableEntity>(
                    filter: $"PartitionKey eq '{productId}'");

                await foreach (var entity in query)
                {
                    subscriptions.Add(new ProductSubscription
                    {
                        ProductId = entity.PartitionKey,
                        Email = entity.RowKey,
                        ProductName = entity.GetString("ProductName") ?? "",
                        NotifyOnPriceIncrease = entity.GetBoolean("NotifyOnPriceIncrease") ?? false,
                        NotifyOnPriceDecrease = entity.GetBoolean("NotifyOnPriceDecrease") ?? true,
                        SubscribedAt = entity.GetDateTime("SubscribedAt") ?? DateTime.UtcNow
                    });
                }

                return subscriptions;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ 查詢訂閱者失敗: {ex.Message}");
                return new List<ProductSubscription>();
            }
        }
    }
}