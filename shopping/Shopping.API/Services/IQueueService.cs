using Shopping.API.Models;

namespace Shopping.API.Services
{
    /// <summary>
    /// Queue 服務介面 - 負責發送訊息到 Azure Queue
    /// </summary>
    public interface IQueueService
    {
        /// <summary>
        /// 發送價格變動訊息到 Queue
        /// </summary>
        /// <param name="message">價格變動的詳細資訊</param>
        Task SendPriceChangeMessageAsync(PriceChangeMessage message);
    }
}