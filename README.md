# MDM API OpenAPI + Mock

這個資料夾包含 OpenAPI 3.1 規格與 Prism Mock 設定，前端可直接以 Mock 伺服器開發。

## 需求
- Node.js >= 18
- npm

## 安裝
```bash
npm install
```

## 啟動 Mock
```bash
npm run mock
# 預設 http://localhost:4010 ，對應 servers[0]
```

## 可用端點（節錄）
- GET /v1/dashboard
- GET /v1/users/me
- GET /v1/groups
- GET /v1/policies
- GET /v1/policies/{id}
- POST /v1/policies
- PATCH /v1/policies/{id}
- DELETE /v1/policies/{id}
- POST /v1/policies/{id}/clone
- PUT /v1/policies/{id}/groups
- POST /v1/policies/{id}/groups/{groupId}
- DELETE /v1/policies/{id}/groups/{groupId}
- POST /v1/policies/{id}/publish
- GET /v1/jobs/{id}
- GET /v1/policy-types
- GET /v1/policy-types/{type}
- GET /v1/firmware/bios/models

## 範例請求
```bash
curl -H "Authorization: Bearer test" http://localhost:4010/v1/dashboard
curl -H "Authorization: Bearer test" http://localhost:4010/v1/policies?page=1&limit=20
```

> 備註
> - 目前以 page/limit 分頁；未來如需 cursor，將新增 `cursor` 與 `nextCursor` 規格。
> - 建立 Policy 預設 `publish=true`，會回傳 `status=publishing` 並提供 Job 查詢.
