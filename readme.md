# Shopping Microservices - 學習專案

## 專案簡介
購物網站微服務架構，包含前端、API 和資料庫，部署在 Azure Kubernetes Service。

**線上展示(開機時間為每日9:00~18:00)：** https://shopping.voidspace.win/

**GitHub連結：** https://github.com/078void/devops_shopping

## 技術
- **語言：** C# (.NET)
- **容器化：** Docker
- **編排：** Kubernetes
- **CI/CD：** GitHub Actions
- **雲端：** Microsoft Azure (ACR, AKS)
- **資料庫：** MongoDB

## 架構圖
```text
[Client (MVC)] → [API (REST)] → [MongoDB]
      ↓              ↓             ↓
   [Docker]        [Docker]       [Docker]
      ↓              ↓             ↓
        └────── Kubernetes (AKS) ──────┘
                      ↓
            [GitHub Actions CI/CD]
```
### 部署流程
- 推送程式碼到 GitHub
- GitHub Actions 自動觸發
- 建置 Docker Images
- 推送到 Azure Container Registry
- 部署到 Azure Kubernetes Service

## 學習成果

- [x] 建立完整的 .NET 微服務架構
- [x] 使用 Docker 容器化應用程式
- [x] 部署容器化服務至 Kubernetes(AKS)
- [x] 撰寫基礎單元測試
- [x] 實作 CI/CD Pipeline
- [ ] Terraform 多環境管理
- [ ] 整合 Azure Monitor 監控
