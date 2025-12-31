using Azure.Storage.Queues;
using Microsoft.Extensions.Options;
using Shopping.API.Settings;
using Shopping.API.Models;
using System.Text.Json;

namespace Shopping.API.Services
{
    /// <summary>
    /// Queue 服務實作 - 實際執行發送訊息的邏輯
    /// </summary>
    public class QueueService : IQueueService
    {
        private readonly QueueClient _priceChangeQueue;
        private readonly ILogger<QueueService> _logger;

        public QueueService(
            IOptions<AzureStorageSettings> settings,
            ILogger<QueueService> logger)
        {
            _logger = logger;
            
            // 建立連線到 Azure Queue 的客戶端
            // "price-change-queue" 是信箱的名字
            _priceChangeQueue = new QueueClient(
                settings.Value.ConnectionString,
                "price-change-queue");
            
            // 如果這個信箱不存在，就建立一個
            _priceChangeQueue.CreateIfNotExists();
            
            _logger.LogInformation("✅ Queue Service 已初始化");
        }

        /// <summary>
        /// 發送價格變動訊息
        /// </summary>
        public async Task SendPriceChangeMessageAsync(PriceChangeMessage message)
        {
            try
            {
                // 把訊息物件轉成 JSON 字串（小紙條的內容）
                var messageJson = JsonSerializer.Serialize(message);
                
                // 把小紙條丟進信箱
                await _priceChangeQueue.SendMessageAsync(messageJson);
                
                _logger.LogInformation(
                    $"💰 已發送價格變動訊息: {message.ProductName} " +
                    $"從 ${message.OldPrice} → ${message.NewPrice} " +
                    $"({message.ChangePercentage:F2}%)");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ 發送訊息失敗: {ex.Message}");
                throw;
            }
        }
    }
}