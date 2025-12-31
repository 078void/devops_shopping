namespace Shopping.Client.Services
{
    /// <summary>
    /// 圖片上傳服務介面
    /// </summary>
    public interface IImageService
    {
        /// <summary>
        /// 上傳圖片到 API
        /// </summary>
        /// <param name="imageFile">圖片檔案</param>
        /// <returns>圖片 URL</returns>
        Task<string?> UploadImageAsync(IFormFile imageFile);
    }
}