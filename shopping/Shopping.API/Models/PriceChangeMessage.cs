namespace Shopping.API.Models
{
    /// <summary>
    /// 價格變動訊息 - 代表一次價格變動的所有資訊
    /// </summary>
    public class PriceChangeMessage
    {
        // 商品 ID
        public string ProductId { get; set; } = string.Empty;
        
        // 商品名稱
        public string ProductName { get; set; } = string.Empty;
        
        // 原本的價格
        public decimal OldPrice { get; set; }
        
        // 新的價格
        public decimal NewPrice { get; set; }
        
        // 變動金額（新價格 - 舊價格）
        public decimal ChangeAmount { get; set; }
        
        // 變動百分比（例如：-8.35 代表降價 8.35%）
        public decimal ChangePercentage { get; set; }
        
        // 誰更新的（目前先寫死 "seller"）
        public string UpdatedBy { get; set; } = "seller";
        
        // 什麼時候更新的
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}