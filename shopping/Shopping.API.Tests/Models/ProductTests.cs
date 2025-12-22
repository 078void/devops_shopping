using FluentAssertions;
using Shopping.API.Models;

namespace Shopping.API.Tests.Models;

/// <summary>
/// Product 模型的單元測試
/// 測試產品模型的基本屬性和驗證
/// </summary>
public class ProductTests
{
    /// <summary>
    /// 測試產品的建立與屬性設定
    /// </summary>
    [Fact(DisplayName = "Product - 應該能正確建立並設定所有屬性")]
    public void Product_ShouldCreateWithAllProperties()
    {
        // Arrange & Act - 建立產品物件
        var product = new Product
        {
            Id = "123",
            Name = "iPhone 15 Pro",
            Category = "智慧型手機",
            Description = "最新款 iPhone，配備 A17 Pro 晶片",
            ImageFile = "iphone15pro.jpg",
            Price = 35000
        };

        // Assert - 驗證所有屬性
        product.Id.Should().Be("123");
        product.Name.Should().Be("iPhone 15 Pro");
        product.Category.Should().Be("智慧型手機");
        product.Description.Should().Be("最新款 iPhone，配備 A17 Pro 晶片");
        product.ImageFile.Should().Be("iphone15pro.jpg");
        product.Price.Should().Be(35000);
    }

    /// <summary>
    /// 測試產品價格應為非負數
    /// </summary>
    [Fact(DisplayName = "Product - 價格應該能設定為正數")]
    public void Product_Price_ShouldBePositive()
    {
        // Arrange
        var product = new Product();

        // Act
        product.Price = 1000;

        // Assert
        product.Price.Should().BeGreaterThan(0);
    }
}