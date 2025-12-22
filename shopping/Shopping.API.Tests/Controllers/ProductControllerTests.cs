using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Moq;
using Shopping.API.Controllers;
using Shopping.API.Data;
using Shopping.API.Models;

namespace Shopping.API.Tests.Controllers;

/// <summary>
/// ProductController 的單元測試
/// 使用 Moq 來模擬相依性，並使用 FluentAssertions 進行斷言
/// </summary>
public class ProductControllerTests
{
    private readonly Mock<IProductContext> _mockContext;
    private readonly Mock<ILogger<ProductController>> _mockLogger;
    private readonly Mock<IMongoCollection<Product>> _mockCollection;
    private readonly ProductController _controller;

    /// <summary>
    /// 測試建構函式 - 在每個測試前執行
    /// 初始化所有需要的 Mock 物件
    /// </summary>
    public ProductControllerTests()
    {
        // 創建 Mock 物件
        _mockContext = new Mock<IProductContext>();
        _mockLogger = new Mock<ILogger<ProductController>>();
        _mockCollection = new Mock<IMongoCollection<Product>>();

        // 設定 Context 回傳 Mock Collection
        _mockContext.Setup(x => x.Products).Returns(_mockCollection.Object);

        // 創建被測試的 Controller
        _controller = new ProductController(_mockContext.Object, _mockLogger.Object);
    }

    #region GetProducts 測試

    /// <summary>
    /// 測試 GetProducts - 成功取得產品列表
    /// </summary>
    [Fact(DisplayName = "GetProducts - 應該回傳所有產品列表")]
    public async Task GetProducts_ShouldReturnOkWithProducts()
    {
        // Arrange - 準備測試資料
        var expectedProducts = new List<Product>
        {
            new Product { Id = "1", Name = "iPhone 14", Category = "手機", Price = 30000 },
            new Product { Id = "2", Name = "Samsung Galaxy", Category = "手機", Price = 25000 }
        };

        // 設定 Mock Find 方法
        var mockCursor = new Mock<IAsyncCursor<Product>>();
        mockCursor.Setup(_ => _.Current).Returns(expectedProducts);
        mockCursor
            .SetupSequence(_ => _.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(true)
            .Returns(false);
        mockCursor
            .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);

        _mockCollection
            .Setup(x => x.FindAsync(
                It.IsAny<FilterDefinition<Product>>(),
                It.IsAny<FindOptions<Product, Product>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockCursor.Object);

        // Act - 執行測試方法
        var result = await _controller.GetProducts();

        // Assert - 驗證結果
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var products = okResult?.Value as List<Product>;
        products.Should().HaveCount(2);
        products.Should().BeEquivalentTo(expectedProducts);
    }


    #endregion

    #region GetProduct 測試

    /// <summary>
    /// 測試 GetProduct - 成功取得單一產品
    /// </summary>
    [Fact(DisplayName = "GetProduct - 根據有效ID應回傳產品")]
    public async Task GetProduct_WithValidId_ShouldReturnProduct()
    {
        // Arrange
        var productId = "1";
        var expectedProduct = new Product
        {
            Id = productId,
            Name = "測試產品",
            Category = "測試分類",
            Price = 100
        };

        var mockCursor = new Mock<IAsyncCursor<Product>>();
        mockCursor.Setup(_ => _.Current).Returns(new List<Product> { expectedProduct });
        mockCursor
            .SetupSequence(_ => _.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(true)
            .Returns(false);
        mockCursor
            .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);

        _mockCollection
            .Setup(x => x.FindAsync(
                It.IsAny<FilterDefinition<Product>>(),
                It.IsAny<FindOptions<Product, Product>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockCursor.Object);

        // Act
        var result = await _controller.GetProduct(productId);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var product = okResult?.Value as Product;
        product.Should().NotBeNull();
        product?.Id.Should().Be(productId);
        product?.Name.Should().Be("測試產品");
    }


    #endregion

    #region CreateProduct 測試

    /// <summary>
    /// 測試 CreateProduct - 成功新增產品
    /// </summary>
    [Fact(DisplayName = "CreateProduct - 應該成功新增產品並回傳CreatedAtAction")]
    public async Task CreateProduct_ShouldReturnCreatedAtAction()
    {
        // Arrange
        var newProduct = new Product
        {
            Name = "新產品",
            Category = "測試分類",
            Description = "測試描述",
            Price = 500
        };

        _mockCollection
            .Setup(x => x.InsertOneAsync(
                It.IsAny<Product>(),
                It.IsAny<InsertOneOptions>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.CreateProduct(newProduct);

        // Assert
        result.Result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result.Result as CreatedAtActionResult;
        createdResult?.ActionName.Should().Be(nameof(ProductController.GetProduct));

        // 驗證 InsertOneAsync 被呼叫一次
        _mockCollection.Verify(
            x => x.InsertOneAsync(
                It.IsAny<Product>(),
                It.IsAny<InsertOneOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region DeleteProduct 測試

    /// <summary>
    /// 測試 DeleteProduct - 成功刪除產品
    /// </summary>
    [Fact(DisplayName = "DeleteProduct - 成功刪除產品應回傳NoContent")]
    public async Task DeleteProduct_WhenProductExists_ShouldReturnNoContent()
    {
        // Arrange
        var productId = "1";
        var deleteResult = new DeleteResult.Acknowledged(1); // 模擬刪除成功

        _mockCollection
            .Setup(x => x.DeleteOneAsync(
                It.IsAny<FilterDefinition<Product>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(deleteResult);

        // Act
        var result = await _controller.DeleteProduct(productId);

        // Assert
        result.Should().BeOfType<NoContentResult>();

        // 驗證 DeleteOneAsync 被呼叫一次
        _mockCollection.Verify(
            x => x.DeleteOneAsync(
                It.IsAny<FilterDefinition<Product>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }


    #endregion

    #region 黑箱測試 - 輸入驗證

    /// <summary>
    /// 黑箱測試：建立產品時價格為負數應該被拒絕
    /// </summary>
    [Fact(DisplayName = "黑箱測試 - CreateProduct 價格為負數應該回傳 BadRequest")]
    public async Task BlackBox_CreateProduct_WithNegativePrice_ShouldReturnBadRequest()
    {
        // Arrange
        var productWithNegativePrice = new Product
        {
            Name = "測試產品",
            Category = "測試分類",
            Description = "測試描述",
            Price = -100 // 負數價格 - 應該被拒絕
        };

        _mockCollection
            .Setup(x => x.InsertOneAsync(
                It.IsAny<Product>(),
                It.IsAny<InsertOneOptions>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.CreateProduct(productWithNegativePrice);

        // Assert - 預期應該回傳 BadRequest，但目前系統會接受
        // 這個測試目前會失敗，需要在後端添加驗證
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    /// <summary>
    /// 黑箱測試：建立產品時名稱為空應該被拒絕
    /// </summary>
    [Fact(DisplayName = "黑箱測試 - CreateProduct 名稱為空應該回傳 BadRequest")]
    public async Task BlackBox_CreateProduct_WithEmptyName_ShouldReturnBadRequest()
    {
        // Arrange
        var productWithEmptyName = new Product
        {
            Name = "", // 空名稱 - 應該被拒絕
            Category = "測試分類",
            Price = 100
        };

        _mockCollection
            .Setup(x => x.InsertOneAsync(
                It.IsAny<Product>(),
                It.IsAny<InsertOneOptions>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.CreateProduct(productWithEmptyName);

        // Assert - 預期應該回傳 BadRequest
        // 這個測試目前會失敗，需要在後端添加必填欄位驗證
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    /// <summary>
    /// 黑箱測試：更新產品時價格為負數應該被拒絕
    /// </summary>
    [Fact(DisplayName = "黑箱測試 - UpdateProduct 價格為負數應該回傳 BadRequest")]
    public async Task BlackBox_UpdateProduct_WithNegativePrice_ShouldReturnBadRequest()
    {
        // Arrange
        var productId = "1";
        var existingProduct = new Product
        {
            Id = productId,
            Name = "原產品",
            Category = "測試",
            Price = 100
        };

        var updatedProduct = new Product
        {
            Name = "更新產品",
            Category = "測試",
            Price = -50 // 負數價格 - 應該被拒絕
        };

        var mockCursor = new Mock<IAsyncCursor<Product>>();
        mockCursor.Setup(_ => _.Current).Returns(new List<Product> { existingProduct });
        mockCursor
            .SetupSequence(_ => _.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(true)
            .Returns(false);
        mockCursor
            .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);

        _mockCollection
            .Setup(x => x.FindAsync(
                It.IsAny<FilterDefinition<Product>>(),
                It.IsAny<FindOptions<Product, Product>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockCursor.Object);

        var replaceResult = new ReplaceOneResult.Acknowledged(1, 1, null);
        _mockCollection
            .Setup(x => x.ReplaceOneAsync(
                It.IsAny<FilterDefinition<Product>>(),
                It.IsAny<Product>(),
                It.IsAny<ReplaceOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(replaceResult);

        // Act
        var result = await _controller.UpdateProduct(productId, updatedProduct);

        // Assert - 預期應該回傳 BadRequest
        // 這個測試目前會失敗，需要在後端添加驗證
        result.Should().BeOfType<BadRequestObjectResult>();
    }


    /// <summary>
    /// 黑箱測試：建立產品時分類為空應該被拒絕
    /// </summary>
    [Fact(DisplayName = "黑箱測試 - CreateProduct 分類為空應該回傳 BadRequest")]
    public async Task BlackBox_CreateProduct_WithEmptyCategory_ShouldReturnBadRequest()
    {
        // Arrange
        var productWithEmptyCategory = new Product
        {
            Name = "測試產品",
            Category = "", // 空分類 - 應該被拒絕
            Price = 100
        };

        _mockCollection
            .Setup(x => x.InsertOneAsync(
                It.IsAny<Product>(),
                It.IsAny<InsertOneOptions>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.CreateProduct(productWithEmptyCategory);

        // Assert - 預期應該回傳 BadRequest
        // 這個測試目前會失敗，需要在後端添加必填欄位驗證
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion
}

