namespace Shopping.API.Settings
{
    /// <summary>
    /// Azure Storage 連線設定
    /// </summary>
    public class AzureStorageSettings
    {
        /// <summary>
        /// Storage Account 連線字串
        /// 格式: DefaultEndpointsProtocol=https;AccountName=你的帳號;AccountKey=你的金鑰;EndpointSuffix=core.windows.net
        /// </summary>
        public string ConnectionString { get; set; } = string.Empty;

        /// <summary>
        /// Blob Container 名稱 - 用來存放商品圖片
        /// </summary>
        public string ProductImagesContainer { get; set; } = "product-images";

    /// <summary>
    /// Queue 名稱 - 用來處理訂單通知
    /// </summary>
    public string OrderQueueName { get; set; } = "order-notifications";

    /// <summary>
    /// 公開的 Blob URL - 用於瀏覽器訪問圖片
    /// 開發環境: http://localhost:10000/devstoreaccount1
    /// 生產環境: https://yourstorageaccount.blob.core.windows.net 或 CDN URL
    /// </summary>
    public string PublicBlobUrl { get; set; } = string.Empty;
}
}