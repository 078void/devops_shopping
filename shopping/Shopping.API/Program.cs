using Shopping.API.Data;
using Shopping.API.Repositories;  
using Shopping.API.Settings; 
using Scalar.AspNetCore;

using Shopping.API.Services;
var builder = WebApplication.CreateBuilder(args);

// ========================================
//  註冊服務到 DI 容器
// ========================================

// 配置綁定：將 appsettings.json 的 DatabaseSettings 區段綁定到類別
builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection("DatabaseSettings"));

// 註冊控制器服務
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// 註冊 ProductContext 為 Scoped 生命週期
builder.Services.AddScoped<IProductContext, ProductContext>();
//  註冊 Repository 層（將介面與實作綁定）
builder.Services.AddScoped<IProductRepository, ProductRepository>();

// 註冊 OpenAPI 服務（.NET 10 新方式）
builder.Services.AddOpenApi();

// 註冊 Azure Storage 設定
builder.Services.Configure<AzureStorageSettings>(
    builder.Configuration.GetSection("AzureStorage"));

// 註冊 Blob Storage 服務
builder.Services.AddSingleton<IBlobStorageService, BlobStorageService>();
// 註冊 Queue 服務
builder.Services.AddSingleton<IQueueService, QueueService>();
// 註冊訂閱服務
builder.Services.AddSingleton<ISubscriptionService, SubscriptionService>();

var app = builder.Build();

// ========================================
//  設定 HTTP 請求管線（Middleware）
// ========================================

// 開發環境啟用 OpenAPI 端點
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();  // 提供 OpenAPI JSON 端點
    app.MapScalarApiReference();  // 提供 Scalar UI 介面
    
}

// 將 HTTP 重導向為 HTTPS（本地開發可先註解掉）
// app.UseHttpsRedirection();

// 啟用路由
app.UseRouting();

// 啟用授權（之後需要時再用）
app.UseAuthorization();

// 對應控制器路由
app.MapControllers();

app.Run();