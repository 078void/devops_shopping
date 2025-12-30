using Microsoft.AspNetCore.Mvc;
using Shopping.API.Services;

namespace Shopping.API.Controllers
{
    /// <summary>
    /// 圖片上傳 API 控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ImageController : ControllerBase
    {
        private readonly IBlobStorageService _blobStorageService;
        private readonly ILogger<ImageController> _logger;

        // 允許的圖片格式
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private const long MaxFileSize = 5 * 1024 * 1024; // 5MB

        public ImageController(
            IBlobStorageService blobStorageService,
            ILogger<ImageController> logger)
        {
            _blobStorageService = blobStorageService;
            _logger = logger;
        }

        /// <summary>
        /// 上傳商品圖片
        /// POST: api/image/upload
        /// </summary>
        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            try
            {
                // 驗證檔案是否存在
                if (file == null || file.Length == 0)
                {
                    return BadRequest("請選擇要上傳的圖片");
                }

                // 驗證檔案大小
                if (file.Length > MaxFileSize)
                {
                    return BadRequest($"檔案大小不能超過 {MaxFileSize / 1024 / 1024}MB");
                }

                // 驗證檔案格式
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!_allowedExtensions.Contains(extension))
                {
                    return BadRequest($"只允許上傳以下格式的圖片: {string.Join(", ", _allowedExtensions)}");
                }

                // 上傳到 Azure Blob Storage
                using var stream = file.OpenReadStream();
                var imageUrl = await _blobStorageService.UploadImageAsync(
                    stream,
                    file.FileName,
                    file.ContentType
                );

                _logger.LogInformation($"圖片上傳成功: {imageUrl}");

                return Ok(new
                {
                    message = "圖片上傳成功",
                    imageUrl = imageUrl,
                    fileName = file.FileName
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"上傳圖片時發生錯誤: {ex.Message}");
                return StatusCode(500, "上傳圖片失敗");
            }
        }

        /// <summary>
        /// 刪除圖片
        /// DELETE: api/image/{fileName}
        /// </summary>
        [HttpDelete("{fileName}")]
        public async Task<IActionResult> DeleteImage(string fileName)
        {
            try
            {
                await _blobStorageService.DeleteImageAsync(fileName);
                return Ok(new { message = "圖片刪除成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"刪除圖片時發生錯誤: {ex.Message}");
                return StatusCode(500, "刪除圖片失敗");
            }
        }
    }
}