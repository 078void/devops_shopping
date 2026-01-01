using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Shopping.Client.Models;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Shopping.Client.Models;
namespace Shopping.Client.Controllers;

/// <summary>
/// 首頁控制器 - 負責顯示產品列表
/// </summary>
public class HomeController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HomeController> _logger;

    private readonly Shopping.Client.Services.IImageService _imageService;

    /// <summary>
    /// 建構函式：注入 HttpClient 和 Logger
    /// </summary>
    public HomeController(IHttpClientFactory httpClientFactory, ILogger<HomeController> logger, Shopping.Client.Services.IImageService imageService)
    {
        // 從 HttpClientFactory 取得已設定的 HttpClient
        _httpClient = httpClientFactory.CreateClient("ShoppingAPIClient");
        _logger = logger;
        _imageService = imageService;
    }

    /// <summary>
    /// 首頁：顯示產品列表
    /// GET: /
    /// </summary>
    public async Task<IActionResult> Index()
    {
        try
        {
            _logger.LogInformation("正在從 API 取得產品列表...");

            // 呼叫 Shopping.API 的 /api/product 端點
            var response = await _httpClient.GetAsync("/api/product");

            // 確認回應成功
            response.EnsureSuccessStatusCode();

            // 讀取回應內容
            var content = await response.Content.ReadAsStringAsync();

            // 將 JSON 反序列化為 Product 清單
            var products = JsonConvert.DeserializeObject<List<Product>>(content)
                ?? new List<Product>();

            _logger.LogInformation($"成功取得 {products.Count} 筆產品資料");

            // 將產品清單傳給 View
            return View(products);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError($"呼叫 API 時發生錯誤: {ex.Message}");
            // 發生錯誤時回傳空清單
            return View(new List<Product>());
        }
        catch (Exception ex)
        {
            _logger.LogError($"發生未預期的錯誤: {ex.Message}");
            return View(new List<Product>());
        }
    }

    /// <summary>
    /// 顯示新增產品表單
    /// GET: /Home/Create
    /// </summary>
    public IActionResult Create()
    {
        // 建立空白的產品物件（不設定預設值，避免驗證問題）
        var product = new Product
        {
            Name = "",
            Category = "",
            Description = "",
            ImageFile = "",
            Price = 0
        };

        return View(product);
    }

    /// <summary>
    /// 處理新增產品請求
    /// POST: /Home/Create
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product, IFormFile imageFile)
    {
        try
        {
            // 記錄接收到的資料（調試用）
            _logger.LogInformation($"Create POST 接收到的資料: Name={product.Name}, Category={product.Category}, Price={product.Price}, ImageFile={product.ImageFile}");

            // 🔧 移除 imageFile 欄位的驗證錯誤（圖片是選填的）
            if (ModelState.ContainsKey("imageFile"))
            {
                ModelState.Remove("imageFile");
                _logger.LogInformation("已移除 imageFile 的驗證錯誤（圖片為選填）");
            }

            if (!ModelState.IsValid)
            {
                // 記錄驗證錯誤
                foreach (var error in ModelState)
                {
                    if (error.Value.Errors.Count > 0)
                    {
                        _logger.LogWarning($"驗證錯誤 - {error.Key}: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                    }
                }

                TempData["ErrorMessage"] = "資料驗證失敗，請檢查必填欄位";
                return View(product);
            }
            if (imageFile != null && imageFile.Length > 0)
            {
                _logger.LogInformation($"正在上傳圖片: {imageFile.FileName}");
                var imageUrl = await _imageService.UploadImageAsync(imageFile);

                if (!string.IsNullOrEmpty(imageUrl))
                {
                    product.ImageFile = imageUrl;
                    _logger.LogInformation($"圖片上傳成功: {imageUrl}");
                }
                else
                {
                    _logger.LogWarning("圖片上傳失敗，圖片將為空");
                    product.ImageFile = string.Empty;
                }
            }
            else
            {
                // 沒有上傳圖片時，保持為空
                product.ImageFile = string.Empty;
                _logger.LogInformation("未上傳圖片");
            }

            // 確保 Description 不是 null（允許空字串）
            product.Description ??= string.Empty;

            // 將產品序列化為 JSON
            var json = JsonConvert.SerializeObject(product);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // 呼叫 API 新增產品
            var response = await _httpClient.PostAsync("/api/product", content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation($"成功新增產品: {product.Name}");
                TempData["SuccessMessage"] = $"成功新增產品：{product.Name}";
                return RedirectToAction(nameof(Index));
            }

            _logger.LogError($"新增產品失敗，狀態碼: {response.StatusCode}");
            ModelState.AddModelError("", "新增產品失敗");
            return View(product);
        }
        catch (Exception ex)
        {
            _logger.LogError($"新增產品時發生錯誤: {ex.Message}");
            ModelState.AddModelError("", $"發生錯誤: {ex.Message}");
            return View(product);
        }
    }

    /// <summary>
    /// 顯示編輯產品表單
    /// GET: /Home/Edit/{id}
    /// </summary>
    public async Task<IActionResult> Edit(string id)
    {
        try
        {
            // 從 API 取得產品
            var response = await _httpClient.GetAsync($"/api/product/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "找不到該產品";
                return RedirectToAction(nameof(Index));
            }

            var content = await response.Content.ReadAsStringAsync();
            var product = JsonConvert.DeserializeObject<Product>(content);

            return View(product);
        }
        catch (Exception ex)
        {
            _logger.LogError($"取得產品 {id} 時發生錯誤: {ex.Message}");
            TempData["ErrorMessage"] = "無法載入產品資料";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// 處理編輯產品請求
    /// POST: /Home/Edit/{id}
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, Product product, IFormFile imageFile)
    {
        try
        {
            // 🔧 移除 imageFile 欄位的驗證錯誤（圖片是選填的）
            if (ModelState.ContainsKey("imageFile"))
            {
                ModelState.Remove("imageFile");
                _logger.LogInformation("已移除 imageFile 的驗證錯誤（圖片為選填）");
            }

            if (!ModelState.IsValid)
            {
                // 記錄驗證錯誤
                foreach (var error in ModelState)
                {
                    if (error.Value.Errors.Count > 0)
                    {
                        _logger.LogWarning($"編輯驗證錯誤 - {error.Key}: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                    }
                }

                TempData["ErrorMessage"] = "資料驗證失敗，請檢查必填欄位";
                return View(product);
            }

            // 確保 ID 一致
            product.Id = id;

            // 處理圖片上傳
            if (imageFile != null && imageFile.Length > 0)
            {
                _logger.LogInformation($"正在上傳新圖片: {imageFile.FileName}");
                var imageUrl = await _imageService.UploadImageAsync(imageFile);

                if (!string.IsNullOrEmpty(imageUrl))
                {
                    product.ImageFile = imageUrl;
                    _logger.LogInformation($"新圖片上傳成功: {imageUrl}");
                }
                else
                {
                    _logger.LogWarning("圖片上傳失敗，保留原有圖片");
                    // 如果上傳失敗，保留原有的 ImageFile（從表單的隱藏欄位）
                }
            }
            else
            {
                // 沒有上傳新圖片時，保留表單提交的 ImageFile 值
                // 這個值來自隱藏欄位，可能是：
                // 1. 原有圖片 URL（使用者沒有更動）
                // 2. 空字串（使用者主動點擊「移除圖片」）
                _logger.LogInformation($"未上傳新圖片，保留表單中的圖片值: '{product.ImageFile ?? "(null)"}'");
                // product.ImageFile 保持不變（來自表單的隱藏欄位）
            }

            // 確保 Description 和 ImageFile 不是 null（將 null 轉為空字串）
            product.Description ??= string.Empty;
            product.ImageFile ??= string.Empty;

            _logger.LogInformation($"最終要更新的圖片值: '{product.ImageFile}'");

            // 將產品序列化為 JSON
            var json = JsonConvert.SerializeObject(product);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // 呼叫 API 更新產品
            var response = await _httpClient.PutAsync($"/api/product/{id}", content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation($"成功更新產品: {product.Name} (ID: {id})");
                TempData["SuccessMessage"] = $"成功更新產品：{product.Name}";
                return RedirectToAction(nameof(Index));
            }

            _logger.LogError($"更新產品失敗，狀態碼: {response.StatusCode}");
            ModelState.AddModelError("", "更新產品失敗");
            return View(product);
        }
        catch (Exception ex)
        {
            _logger.LogError($"更新產品 {id} 時發生錯誤: {ex.Message}");
            ModelState.AddModelError("", $"發生錯誤: {ex.Message}");
            return View(product);
        }
    }

    /// <summary>
    /// 刪除產品
    /// POST: /Home/Delete/{id}
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            // 呼叫 API 刪除產品
            var response = await _httpClient.DeleteAsync($"/api/product/{id}");

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation($"成功刪除產品 ID: {id}");
                TempData["SuccessMessage"] = "成功刪除產品";
            }
            else
            {
                _logger.LogError($"刪除產品失敗，狀態碼: {response.StatusCode}");
                TempData["ErrorMessage"] = "刪除產品失敗";
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError($"刪除產品 {id} 時發生錯誤: {ex.Message}");
            TempData["ErrorMessage"] = $"發生錯誤: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// 訂閱商品價格通知（轉發到 API）
    /// POST: /api/subscription
    /// </summary>
    [HttpPost]
    [Route("api/subscription")]
    public async Task<IActionResult> Subscribe([FromBody] ProductSubscription subscriptionData)
    {
        try
        {
            _logger.LogInformation("轉發訂閱請求到 Shopping.API");
            _logger.LogInformation($"收到的資料 - ProductId: {subscriptionData.ProductId}, Email: {subscriptionData.Email}, ProductName: {subscriptionData.ProductName}");
            var json = JsonConvert.SerializeObject(subscriptionData);
            _logger.LogInformation($"序列化後的 JSON: {json}");
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("/api/subscription", content);
            var responseContent = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                return Ok(JsonConvert.DeserializeObject(responseContent));
            }
            
            return StatusCode((int)response.StatusCode, JsonConvert.DeserializeObject(responseContent));
        }
        catch (Exception ex)
        {
            _logger.LogError($"訂閱失敗: {ex.Message}");
            return StatusCode(500, new { message = "訂閱失敗，請稍後再試" });
        }
    }
    /// <summary>
    /// 隱私權政策頁面
    /// </summary>
    public IActionResult Privacy()
    {
        return View();
    }

    /// <summary>
    /// 錯誤頁面
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }
}