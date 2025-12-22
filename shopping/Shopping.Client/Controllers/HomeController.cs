using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Shopping.Client.Models;
using System.Diagnostics;
using System.Text;

namespace Shopping.Client.Controllers;

/// <summary>
/// 首頁控制器 - 負責顯示產品列表
/// </summary>
public class HomeController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HomeController> _logger;

    /// <summary>
    /// 建構函式：注入 HttpClient 和 Logger
    /// </summary>
    public HomeController(IHttpClientFactory httpClientFactory, ILogger<HomeController> logger)
    {
        // 從 HttpClientFactory 取得已設定的 HttpClient
        _httpClient = httpClientFactory.CreateClient("ShoppingAPIClient");
        _logger = logger;
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
        // 建立有預設值的產品物件
        var product = new Product
        {
            Name = "iPhone 15 Pro Max",
            Category = "Smart Phone",
            Description = "最新款旗艦智慧型手機，搭載強大性能處理器",
            ImageFile = "product-default.png",
            Price = 29999
        };
        
        return View(product);
    }

    /// <summary>
    /// 處理新增產品請求
    /// POST: /Home/Create
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return View(product);
            }

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
    public async Task<IActionResult> Edit(string id, Product product)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return View(product);
            }

            // 確保 ID 一致
            product.Id = id;

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