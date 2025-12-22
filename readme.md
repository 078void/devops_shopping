## 本地啟動方法
---

## 🚀 快速啟動

### 1️⃣ 啟動 MongoDB
```bash
docker start shopping-mongo
```

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