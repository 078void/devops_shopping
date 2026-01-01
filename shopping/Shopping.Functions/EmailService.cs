using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Logging;

namespace Shopping.Functions
{
    /// <summary>
    /// Email 發送服務
    /// </summary>
    public class EmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUser;
        private readonly string _smtpPassword;
        private readonly string _fromEmail;
        private readonly string _fromName;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
            
            // 從環境變數讀取設定
            _smtpHost = Environment.GetEnvironmentVariable("SmtpHost") ?? "";
            _smtpPort = int.Parse(Environment.GetEnvironmentVariable("SmtpPort") ?? "587");
            _smtpUser = Environment.GetEnvironmentVariable("SmtpUser") ?? "";
            _smtpPassword = Environment.GetEnvironmentVariable("SmtpPassword") ?? "";
            _fromEmail = Environment.GetEnvironmentVariable("SmtpFromEmail") ?? "";
            _fromName = Environment.GetEnvironmentVariable("SmtpFromName") ?? "系統通知";
        }

        /// <summary>
        /// 發送價格變動通知 Email
        /// </summary>
        public async Task SendPriceAlertAsync(
            string toEmail, 
            string productName,
            decimal oldPrice,
            decimal newPrice,
            decimal changePercentage,
            bool isPriceIncrease)
        {
            try
            {
                var message = new MimeMessage();
                
                // 寄件者
                message.From.Add(new MailboxAddress(_fromName, _fromEmail));
                
                // 收件者
                message.To.Add(new MailboxAddress(toEmail, toEmail));
                
                // 主旨
                var alertType = isPriceIncrease ? "🔺 價格上漲" : "🔻 價格下降";
                message.Subject = $"{alertType} 通知：{productName}";
                
                // 內容（HTML 格式）
                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $@"
                        <html>
                        <head>
                            <style>
                                body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                                .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                                .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); 
                                          color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
                                .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
                                .price-box {{ background: white; padding: 20px; margin: 20px 0; 
                                             border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }}
                                .old-price {{ text-decoration: line-through; color: #999; font-size: 18px; }}
                                .new-price {{ color: {(isPriceIncrease ? "#e74c3c" : "#27ae60")}; 
                                             font-size: 32px; font-weight: bold; }}
                                .change {{ color: {(isPriceIncrease ? "#e74c3c" : "#27ae60")}; 
                                          font-size: 24px; font-weight: bold; }}
                                .footer {{ text-align: center; color: #999; font-size: 12px; margin-top: 20px; }}
                                .button {{ display: inline-block; padding: 12px 24px; background: #667eea; 
                                          color: white; text-decoration: none; border-radius: 5px; margin-top: 20px; }}
                            </style>
                        </head>
                        <body>
                            <div class='container'>
                                <div class='header'>
                                    <h1>{alertType}</h1>
                                    <p>您訂閱的商品價格有變動！</p>
                                </div>
                                <div class='content'>
                                    <h2>📦 {productName}</h2>
                                    
                                    <div class='price-box'>
                                        <p><strong>原價：</strong><span class='old-price'>NT$ {oldPrice:N0}</span></p>
                                        <p><strong>新價：</strong><span class='new-price'>NT$ {newPrice:N0}</span></p>
                                        <hr>
                                        <p><strong>變動：</strong><span class='change'>{(isPriceIncrease ? "+" : "")}{changePercentage:F1}%</span></p>
                                    </div>
                                    
                                    <p style='text-align: center;'>
                                        <a href='https://shopping.voidspace.win/' class='button'>
                                            立即查看商品
                                        </a>
                                    </p>
                                    
                                    <div class='footer'>
                                        <p>這是系統自動發送的通知郵件</p>
                                        <p>如需取消訂閱，請登入網站管理訂閱設定</p>
                                    </div>
                                </div>
                            </div>
                        </body>
                        </html>
                    "
                };
                
                message.Body = bodyBuilder.ToMessageBody();
                
                // 發送郵件
                using var client = new SmtpClient();
                await client.ConnectAsync(_smtpHost, _smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_smtpUser, _smtpPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
                
                _logger.LogInformation($"✅ Email 已發送到 {toEmail}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ 發送 Email 失敗: {ex.Message}");
                throw;
            }
        }
    }
}