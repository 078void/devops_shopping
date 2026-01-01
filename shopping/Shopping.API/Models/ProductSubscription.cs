namespace Shopping.API.Models
{
    /// <summary>
    /// 商品價格變動訂閱
    /// </summary>
    public class ProductSubscription
    {
        public string? Id { get; set; }
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool NotifyOnPriceIncrease { get; set; } = false;  // 漲價時通知
        public bool NotifyOnPriceDecrease { get; set; } = true;   // 降價時通知
        public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;
    }
}