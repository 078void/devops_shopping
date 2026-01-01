using System.ComponentModel.DataAnnotations;

namespace Shopping.Client.Models;

/// <summary>
/// 產品資料模型 - 用於接收 API 回傳的產品資料
/// </summary>
public class Product
{
    /// <summary>產品 ID</summary>
    public string? Id { get; set; }

    /// <summary>產品名稱</summary>
    [Required(ErrorMessage = "產品名稱為必填")]
    [StringLength(100, ErrorMessage = "產品名稱最多 100 個字元")]
    public string Name { get; set; } = string.Empty;

    /// <summary>產品類別</summary>
    [Required(ErrorMessage = "產品類別為必填")]
    public string Category { get; set; } = string.Empty;

    /// <summary>產品描述（選填）</summary>
    [StringLength(500, ErrorMessage = "產品描述最多 500 個字元")]
    public string? Description { get; set; } = string.Empty;

    /// <summary>產品圖片檔名（選填）</summary>
    public string? ImageFile { get; set; } = string.Empty;

    /// <summary>產品價格</summary>
    [Required(ErrorMessage = "產品價格為必填")]
    [Range(0.01, 999999.99, ErrorMessage = "價格必須介於 0.01 到 999999.99 之間")]
    public decimal Price { get; set; }
}