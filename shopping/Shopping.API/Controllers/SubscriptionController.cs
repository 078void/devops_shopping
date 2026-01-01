using Microsoft.AspNetCore.Mvc;
using Shopping.API.Models;
using Shopping.API.Services;

namespace Shopping.API.Controllers
{
    /// <summary>
    /// 訂閱 API 控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class SubscriptionController : ControllerBase
    {
        private readonly ISubscriptionService _subscriptionService;
        private readonly ILogger<SubscriptionController> _logger;

        public SubscriptionController(
            ISubscriptionService subscriptionService,
            ILogger<SubscriptionController> logger)
        {
            _subscriptionService = subscriptionService;
            _logger = logger;
        }

        /// <summary>
        /// 訂閱商品價格變動通知
        /// POST: api/subscription
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Subscribe([FromBody] ProductSubscription subscription)
        {
            _logger.LogInformation("===== 收到訂閱請求 =====");
            _logger.LogInformation($"Subscription 是否為 null: {subscription == null}");
            if (subscription != null)
            {
                _logger.LogInformation($"ProductId: {subscription.ProductId}");
                _logger.LogInformation($"ProductName: {subscription.ProductName}");
                _logger.LogInformation($"Email: {subscription.Email}");
                _logger.LogInformation($"NotifyOnPriceIncrease: {subscription.NotifyOnPriceIncrease}");
                _logger.LogInformation($"NotifyOnPriceDecrease: {subscription.NotifyOnPriceDecrease}");
            }
            _logger.LogInformation("========================");

            // 驗證 Email 格式
            if (string.IsNullOrEmpty(subscription.Email) || 
                !subscription.Email.Contains("@"))
            {
                _logger.LogError($"無效的 Email: {subscription.Email}");
                return BadRequest(new { message = "請輸入有效的 Email" });
            }

            // 驗證 ProductId
            if (string.IsNullOrEmpty(subscription.ProductId))
            {
                _logger.LogWarning("ProductId 驗證失敗");
                return BadRequest(new { message = "請指定商品" });
            }

            var success = await _subscriptionService.SubscribeAsync(subscription);
            
            if (success)
            {
                return Ok(new 
                { 
                    message = "訂閱成功！當價格變動時會寄送通知到您的信箱",
                    email = subscription.Email,
                    productName = subscription.ProductName
                });
            }
            
            return StatusCode(500, new { message = "訂閱失敗，請稍後再試" });
        }

        /// <summary>
        /// 取消訂閱
        /// DELETE: api/subscription?email=xxx&productId=xxx
        /// </summary>
        [HttpDelete]
        public async Task<IActionResult> Unsubscribe(
            [FromQuery] string email, 
            [FromQuery] string productId)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(productId))
            {
                return BadRequest(new { message = "缺少必要參數" });
            }

            var success = await _subscriptionService.UnsubscribeAsync(email, productId);
            
            if (success)
            {
                return Ok(new { message = "已取消訂閱" });
            }
            
            return StatusCode(500, new { message = "取消訂閱失敗" });
        }
    }
}