# ========================================
# 階段 1: 建置階段 (Build Stage)
# ========================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 複製專案檔並還原套件（先複製 .csproj 可以利用 Docker 快取）
COPY ["Shopping.API.csproj", "./"]
RUN dotnet restore "Shopping.API.csproj"

# 複製所有原始碼
COPY . .

# 建置專案並發佈到 /app/publish
RUN dotnet build "Shopping.API.csproj" -c Release -o /app/build
RUN dotnet publish "Shopping.API.csproj" -c Release -o /app/publish

# ========================================
# 階段 2: 執行階段 (Runtime Stage)
# ========================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# 從建置階段複製發佈的檔案
COPY --from=build /app/publish .

# 公開 80 埠（容器內部的埠號）
EXPOSE 80

# 設定環境變數
ENV ASPNETCORE_URLS=http://+:80

# 啟動應用程式
ENTRYPOINT ["dotnet", "Shopping.API.dll"]