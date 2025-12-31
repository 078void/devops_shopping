using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using Shopping.API.Settings;

namespace Shopping.API.Services
{
    /// <summary>
    /// Blob Storage 服務實作
    /// </summary>
    public class BlobStorageService : IBlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly BlobContainerClient _containerClient;
        private readonly AzureStorageSettings _settings;
        private readonly ILogger<BlobStorageService> _logger;

        public BlobStorageService(
            IOptions<AzureStorageSettings> settings,
            ILogger<BlobStorageService> logger)
        {
            _settings = settings.Value;
            _logger = logger;

            // 建立 Blob Service Client
            _blobServiceClient = new BlobServiceClient(_settings.ConnectionString);

            // 取得 Container Client（不立即創建）
            _containerClient = _blobServiceClient.GetBlobContainerClient(_settings.ProductImagesContainer);

            _logger.LogInformation($"Blob Storage 服務已初始化，Container: {_settings.ProductImagesContainer}");
        }

        /// <summary>
        /// 上傳圖片到 Azure Blob Storage
        /// </summary>
        public async Task<string> UploadImageAsync(Stream imageStream, string fileName, string contentType)
        {
            try
            {
                try
                {
                    await _containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);
                }
                catch (Azure.RequestFailedException ex) when (ex.Status == 409)
                {
                    // 容器已存在，忽略
                }
        
                // 產生唯一的檔案名稱（避免重複）
                var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
                
                // 取得 Blob Client
                var blobClient = _containerClient.GetBlobClient(uniqueFileName);

                // 設定上傳選項
                var uploadOptions = new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = contentType
                    }
                };

                // 上傳圖片
                await blobClient.UploadAsync(imageStream, uploadOptions);

                _logger.LogInformation($"成功上傳圖片: {uniqueFileName}");

                // 回傳圖片的 URL
                // 如果設定了公開 URL，使用公開 URL（給瀏覽器用）
                // 否則使用 Blob 的內部 URL
                if (!string.IsNullOrEmpty(_settings.PublicBlobUrl))
                {
                    var publicUrl = $"{_settings.PublicBlobUrl}/{_settings.ProductImagesContainer}/{uniqueFileName}";
                    _logger.LogInformation($"返回公開 URL: {publicUrl}");
                    return publicUrl;
                }
                
                return blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError($"上傳圖片失敗: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 刪除圖片
        /// </summary>
        public async Task DeleteImageAsync(string fileName)
        {
            try
            {
                var blobClient = _containerClient.GetBlobClient(fileName);
                await blobClient.DeleteIfExistsAsync();
                _logger.LogInformation($"成功刪除圖片: {fileName}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"刪除圖片失敗: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 取得圖片 URL
        /// </summary>
        public string GetImageUrl(string fileName)
        {
            // 如果設定了公開 URL，使用公開 URL
            if (!string.IsNullOrEmpty(_settings.PublicBlobUrl))
            {
                return $"{_settings.PublicBlobUrl}/{_settings.ProductImagesContainer}/{fileName}";
            }
            
            // 否則使用 Blob 的內部 URL
            var blobClient = _containerClient.GetBlobClient(fileName);
            return blobClient.Uri.ToString();
        }
    }
}