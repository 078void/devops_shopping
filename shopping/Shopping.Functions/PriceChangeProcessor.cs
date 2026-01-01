using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Azure.Data.Tables;
using Azure.Storage.Queues;

namespace Shopping.Functions
{
    /// <summary>
    /// 價格變動處理器
    /// 這個 Function 會自動監聽 price-change-queue
    /// 當有新訊息時，自動執行 Run 方法
    /// </summary>
    public class PriceChangeProcessor
    {
        private readonly ILogger<PriceChangeProcessor> _logger;
        private readonly TableClient _tableClient;
        // 建構函式：注入 Logger（用來輸出訊息）
        public PriceChangeProcessor(ILogger<PriceChangeProcessor> logger)
        {
            _logger = logger;
             // 讀取連線字串
            var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            
            // 建立 Table Client（連接到 PriceHistory 表格）
            _tableClient = new TableClient(connectionString, "PriceHistory");
            
            // 確保 Table 存在（第一次會自動建立）
            _tableClient.CreateIfNotExists();
            
            _logger.LogInformation("✅ Table Client 已初始化");
        }

        /// <summary>
        /// 這個方法會在 Queue 有新訊息時自動執行
        /// [QueueTrigger] 是魔法關鍵字，告訴 Azure 要監聽哪個 Queue
        /// </summary>
        [Function("PriceChangeProcessor")]
        public async Task Run(
            [QueueTrigger("price-change-queue", Connection = "AzureWebJobsStorage")] 
            string queueMessage)
        {
            // ===== 第 1 步：顯示收到訊息 =====
            _logger.LogInformation("========== 開始處理 ==========");
            _logger.LogInformation($"原始訊息長度: {queueMessage?.Length ?? 0}");
            _logger.LogInformation($"原始訊息內容:\n{queueMessage}");
            _logger.LogInformation("==============================");

            try
            {
                // ===== 第 2 步：把 JSON 字串轉成物件 =====
                var message = JsonSerializer.Deserialize<PriceChangeMessage>(queueMessage);
                
                // 檢查訊息是否正確
                if (message == null)
                {
                    _logger.LogError("❌ 訊息格式錯誤，無法解析");
                    return;
                }

                // ===== 第 3 步：顯示商品資訊 =====
                _logger.LogInformation("=================================");
                _logger.LogInformation($"📊 商品名稱: {message.ProductName}");
                _logger.LogInformation($"💰 原本價格: ${message.OldPrice}");
                _logger.LogInformation($"💰 新的價格: ${message.NewPrice}");
                _logger.LogInformation($"📈 變動金額: ${message.ChangeAmount}");
                _logger.LogInformation($"📊 變動百分比: {message.ChangePercentage:F2}%");
                _logger.LogInformation($"⏰ 更新時間: {message.Timestamp}");
                _logger.LogInformation("=================================");

                // ===== 第 4 步：儲存到 Storage Table=====
                _logger.LogInformation("💾 開始儲存價格歷史...");
                
                // 建立 Entity（資料表的一筆記錄）
                // PartitionKey: 用 ProductId（同一個商品的記錄會在同一個分區，查詢更快）
                // RowKey: 用時間戳記（確保唯一性，且按時間排序）
                var entity = new TableEntity(
                    partitionKey: message.ProductId,
                    rowKey: DateTime.UtcNow.Ticks.ToString())
                {
                    { "ProductName", message.ProductName },
                    { "OldPrice", (double)message.OldPrice },  // Table 不支援 decimal，轉成 double
                    { "NewPrice", (double)message.NewPrice },
                    { "ChangeAmount", (double)message.ChangeAmount },
                    { "ChangePercentage", (double)message.ChangePercentage },
                    { "UpdatedBy", message.UpdatedBy },
                    { "ChangeTime", message.Timestamp }
                };

                // 儲存到 Table
                await _tableClient.AddEntityAsync(entity);
                
                _logger.LogInformation("✅ 價格歷史已儲存到 Storage Table");

                // ===== 第 5 步：檢查是否大幅變動 =====
                if (Math.Abs(message.ChangePercentage) >= 20)
                {
                    _logger.LogWarning("🚨🚨🚨 警告：價格變動超過 20%！");
    
                    // 建立警告訊息
                    var alert = new
                    {
                        AlertType = message.ChangePercentage > 0 ? "大幅漲價" : "大幅降價",
                        ProductId = message.ProductId,
                        ProductName = message.ProductName,
                        OldPrice = message.OldPrice,
                        NewPrice = message.NewPrice,
                        ChangeAmount = message.ChangeAmount,
                        ChangePercentage = message.ChangePercentage,
                        AlertTime = DateTime.UtcNow
                    };
                    
                    // 發送到警告 Queue
                    var alertQueue = new Azure.Storage.Queues.QueueClient(
                        Environment.GetEnvironmentVariable("AzureWebJobsStorage"),
                        "price-alert-queue");
                    
                    await alertQueue.CreateIfNotExistsAsync();
                    
                    var alertJson = JsonSerializer.Serialize(alert);
                    await alertQueue.SendMessageAsync(alertJson);
                    
                    _logger.LogWarning($"📨 已發送警告到 price-alert-queue");
                }
            }
            catch (Exception ex)
            {
                // 如果發生錯誤，記錄下來
                _logger.LogError($"❌ 處理訊息時發生錯誤: {ex.Message}");
                _logger.LogError($"❌ 錯誤類型: {ex.GetType().Name}");
                _logger.LogError($"❌ 錯誤堆疊: {ex.StackTrace}");
                throw; // 重新拋出例外，讓 Azure 知道處理失敗
            }
        }
    }

    /// <summary>
    /// 價格變動訊息的資料結構
    /// 必須跟 Shopping.API 裡的 PriceChangeMessage 一模一樣
    /// </summary>
    public class PriceChangeMessage
    {
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public decimal OldPrice { get; set; }
        public decimal NewPrice { get; set; }
        public decimal ChangeAmount { get; set; }
        public decimal ChangePercentage { get; set; }
        public string UpdatedBy { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}