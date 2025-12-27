using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// 配置 Data Protection（共享 Key，適用於多個 Pod）
builder.Services.AddDataProtection()
    .SetApplicationName("ShoppingClient")  // 所有 Pod 使用相同的應用名稱
    .PersistKeysToFileSystem(new DirectoryInfo("/app/dataprotection-keys"));  // 持久化到共享目錄

// 設定轉發標頭支援（適用於反向代理環境）
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// 註冊 HttpClient 用於呼叫 Shopping.API
builder.Services.AddHttpClient("ShoppingAPIClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ShoppingAPIUrl"] 
        ?? "http://localhost:5000");
});

var app = builder.Build();

// 使用轉發標頭（必須在其他 middleware 之前）
app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// 只在開發環境使用 HTTPS 重定向
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();