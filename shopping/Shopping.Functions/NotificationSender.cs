using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Azure.Data.Tables;

namespace Shopping.Functions
{
    public class NotificationSender
    {
        private readonly ILogger<NotificationSender> _logger;
        private readonly TableClient _subscriptionTable;
        private readonly EmailService _emailService;

        public NotificationSender(ILogger<NotificationSender> logger)
        {
            _logger = logger;
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var emailLogger = loggerFactory.CreateLogger<EmailService>();
            _emailService = new EmailService(emailLogger);
            
            // 連接到訂閱表
            var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            _subscriptionTable = new TableClient(connectionString, "ProductSubscriptions");
        }

        [Function("NotificationSender")]
        public async Task Run(
            [QueueTrigger("price-alert-queue", Connection = "AzureWebJobsStorage")]
            string alertMessage)
        {
            _logger.LogWarning("🔔🔔🔔 收到價格警告通知！");

            try
            {
                var alert = JsonSerializer.Deserialize<PriceAlert>(alertMessage);

                if (alert == null)
                {
                    _logger.LogError("❌ 無法解析警告訊息");
                    return;
                }

                // 顯示警告詳情
                _logger.LogWarning("╔════════════════════════════════════════╗");
                _logger.LogWarning($"║ 🚨 {alert.AlertType}警告！");
                _logger.LogWarning($"║ 📦 商品：{alert.ProductName}");
                _logger.LogWarning($"║ 💰 原價：${alert.OldPrice}");
                _logger.LogWarning($"║ 💰 新價：${alert.NewPrice}");
                _logger.LogWarning($"║ 📊 變動：{alert.ChangePercentage:F2}%");
                _logger.LogWarning("╚════════════════════════════════════════╝");

                // 🆕 查詢該商品的所有訂閱者
                var subscribers = await GetSubscribersAsync(alert.ProductId, alert.ChangePercentage > 0);
                
                _logger.LogInformation($"📬 找到 {subscribers.Count} 位訂閱者");

                // 🆕 發送 Email 給每位訂閱者
                foreach (var subscriber in subscribers)
                {
                    try
                    {
                        await _emailService.SendPriceAlertAsync(
                            toEmail: subscriber.Email,
                            productName: alert.ProductName,
                            oldPrice: alert.OldPrice,
                            newPrice: alert.NewPrice,
                            changePercentage: alert.ChangePercentage,
                            isPriceIncrease: alert.ChangePercentage > 0
                        );
                        
                        _logger.LogInformation($"✅ 已通知 {subscriber.Email}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"❌ 通知 {subscriber.Email} 失敗: {ex.Message}");
                    }
                }

                _logger.LogInformation($"✅ 通知處理完成，已發送 {subscribers.Count} 封郵件");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ 處理警告時發生錯誤: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 取得商品的訂閱者
        /// </summary>
        private async Task<List<Subscriber>> GetSubscribersAsync(string productId, bool isPriceIncrease)
        {
            var subscribers = new List<Subscriber>();
            
            try
            {
                var query = _subscriptionTable.QueryAsync<TableEntity>(
                    filter: $"PartitionKey eq '{productId}'");

                await foreach (var entity in query)
                {
                    var notifyOnIncrease = entity.GetBoolean("NotifyOnPriceIncrease") ?? false;
                    var notifyOnDecrease = entity.GetBoolean("NotifyOnPriceDecrease") ?? true;
                    
                    // 根據使用者的訂閱設定決定是否通知
                    bool shouldNotify = isPriceIncrease ? notifyOnIncrease : notifyOnDecrease;
                    
                    if (shouldNotify)
                    {
                        subscribers.Add(new Subscriber
                        {
                            Email = entity.RowKey,
                            ProductName = entity.GetString("ProductName") ?? ""
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ 查詢訂閱者失敗: {ex.Message}");
            }

            return subscribers;
        }
    }

    public class PriceAlert
    {
        public string AlertType { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public decimal OldPrice { get; set; }
        public decimal NewPrice { get; set; }
        public decimal ChangeAmount { get; set; }
        public decimal ChangePercentage { get; set; }
        public DateTime AlertTime { get; set; }
    }

    public class Subscriber
    {
        public string Email { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
    }
}