using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// 🆕 設定轉發標頭支援（適用於反向代理環境）
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // 信任所有代理（適用於 Cloudflare + Ingress）
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// 註冊 HttpClient 用於呼叫 Shopping.API
builder.Services.AddHttpClient("ShoppingAPIClient", client =>
{
    // 設定 API 的基礎位址
    client.BaseAddress = new Uri(builder.Configuration["ShoppingAPIUrl"] 
        ?? "http://localhost:5000");
});

var app = builder.Build();

// 🆕 使用轉發標頭（必須在其他 middleware 之前）
app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// 🔧 只在開發環境使用 HTTPS 重定向
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