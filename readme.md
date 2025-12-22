
## 🐳 Docker 啟動

```bash
# 啟動所有服務（一鍵啟動）
cd shopping
docker-compose up --build

```

🌐 網站: http://localhost:5001  
🌐 API: http://localhost:5000/scalar/v1

---
## 🚀 執行測試

### 執行所有測試
```powershell
cd shopping/Shopping.API.Tests
### 執行所有測試
dotnet test

### 執行測試並顯示詳細資訊
dotnet test --verbosity normal

### 執行測試並產生程式碼覆蓋率報告
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults
```
---

## 本地啟動方法

## 🚀 快速啟動

### 1️⃣ 啟動 MongoDB
開啟MongoDB在localhost:27017上

### 2️⃣ 啟動 Shopping.API（終端視窗 1）
```bash
cd shopping/Shopping.API
dotnet watch run  # 使用 Hot Reload，程式碼變更時自動重新載入
```
🌐 API: http://localhost:5000/scalar/v1

### 3️⃣ 啟動 Shopping.Client（終端視窗 2）
```bash
cd shopping/Shopping.Client
dotnet watch run  # 使用 Hot Reload，程式碼變更時自動重新載入
```
🌐 網站: http://localhost:5001

> 💡 **Hot Reload 模式：** 程式碼修改後會自動重新編譯，不需手動重啟！

---
