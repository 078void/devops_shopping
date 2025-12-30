namespace Shopping.API.Services
{
    /// <summary>
    /// Blob Storage 服務介面 - 處理檔案上傳到 Azure Storage
    /// </summary>
    public interface IBlobStorageService
    {
        /// <summary>
        /// 上傳圖片到 Azure Blob Storage
        /// </summary>
        /// <param name="imageStream">圖片的資料流</param>
        /// <param name="fileName">檔案名稱</param>
        /// <param name="contentType">內容類型（例如：image/jpeg）</param>
        /// <returns>上傳後的圖片 URL</returns>
        Task<string> UploadImageAsync(Stream imageStream, string fileName, string contentType);

        /// <summary>
        /// 刪除圖片
        /// </summary>
        /// <param name="fileName">要刪除的檔案名稱</param>
        Task DeleteImageAsync(string fileName);

        /// <summary>
        /// 取得圖片 URL
        /// </summary>
        /// <param name="fileName">檔案名稱</param>
        /// <returns>圖片的完整 URL</returns>
        string GetImageUrl(string fileName);
    }
}