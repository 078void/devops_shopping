using Shopping.API.Models;

namespace Shopping.API.Services
{
    /// <summary>
    /// 訂閱服務介面
    /// </summary>
    public interface ISubscriptionService
    {
        /// <summary>
        /// 訂閱商品價格變動通知
        /// </summary>
        Task<bool> SubscribeAsync(ProductSubscription subscription);
        
        /// <summary>
        /// 取消訂閱
        /// </summary>
        Task<bool> UnsubscribeAsync(string email, string productId);
        
        /// <summary>
        /// 取得某商品的所有訂閱者
        /// </summary>
        Task<List<ProductSubscription>> GetSubscribersAsync(string productId);
    }
}