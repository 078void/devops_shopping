
### 部署所有服務（一鍵部署）
```powershell
# MongoDB
kubectl apply -f k8s/mongodb-secret.yaml
kubectl apply -f k8s/mongodb-pvc.yaml
kubectl apply -f k8s/mongodb-deployment.yaml
kubectl apply -f k8s/mongodb-service.yaml

# Shopping.API
kubectl apply -f k8s/shoppingapi-configmap.yaml
kubectl apply -f k8s/shoppingapi-deployment.yaml
kubectl apply -f k8s/shoppingapi-service.yaml

# Shopping.Client
kubectl apply -f k8s/shoppingclient-deployment.yaml
kubectl apply -f k8s/shoppingclient-service.yaml
```

### 查看部署狀態
```powershell
kubectl get pods              # 查看 Pods 狀態
kubectl get services          # 查看 Services（含外部 IP）
kubectl get deployments       # 查看 Deployments
```

###  取得外部訪問位址
```powershell
# 取得 LoadBalancer 外部 IP
kubectl get service shoppingclient-service

# 或直接取得 IP
kubectl get service shoppingclient-service -o jsonpath='{.status.loadBalancer.ingress[0].ip}'
```

---
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
