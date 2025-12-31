namespace Shopping.Client.Services
{
    /// <summary>
    /// 圖片上傳服務實作
    /// </summary>
    public class ImageService : IImageService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ImageService> _logger;

        public ImageService(IHttpClientFactory httpClientFactory, ILogger<ImageService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("ShoppingAPIClient");
            _logger = logger;
        }

        /// <summary>
        /// 上傳圖片到 API
        /// </summary>
        public async Task<string?> UploadImageAsync(IFormFile imageFile)
        {
            try
            {
                if (imageFile == null || imageFile.Length == 0)
                {
                    _logger.LogWarning("沒有選擇圖片或圖片為空");
                    return null;
                }

                // 建立 multipart/form-data
                using var content = new MultipartFormDataContent();
                using var fileStream = imageFile.OpenReadStream();
                using var streamContent = new StreamContent(fileStream);
                
                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(imageFile.ContentType);
                content.Add(streamContent, "file", imageFile.FileName);

                // 上傳到 API
                var response = await _httpClient.PostAsync("/api/Image/upload", content);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ImageUploadResponse>();
                    _logger.LogInformation($"圖片上傳成功: {result?.ImageUrl}");
                    return result?.ImageUrl;
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"圖片上傳失敗: {error}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"上傳圖片時發生錯誤: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// API 回應模型
        /// </summary>
        private class ImageUploadResponse
        {
            public string? Message { get; set; }
            public string? ImageUrl { get; set; }
            public string? FileName { get; set; }
        }
    }
}