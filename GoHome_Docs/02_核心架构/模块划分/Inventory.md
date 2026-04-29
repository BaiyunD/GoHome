# Inventory（背?物品?
## 职责
- 维护库存字典与数量变更（Add/Remove?- 提供物品存在/数量查
- UI 展示与交互（使用/详情?
## 权威数据?- `InventoryManager` 内部库存结构（运行时?- `ItemRegistry`：物品制体/定义注册（资源侧?
## 关键文件
- `Assets/Scripts/Manager/InventoryManager.cs`
- `Assets/Scripts/UIs/InventoryPage.cs`
- `Assets/Scripts/UIs/PanelDetailsPanel.cs`
- `Assets/Scripts/Items/PrefabItems/ItemRegistry.cs`

## 常俔跾
- ?UI 列表：从 `InventoryPage.RefreshView/UpdateItems` 入手
- 改情面板：?`PanelDetailsPanel` 的绑定与刷新入手
- 新物品：先补齐资源与注册，再接入事?奖励/掉落逻辑

## 风险与校?- 高险：物品定义与注册不同（UI 能看到但无法使用/反之?- 必查：面板字段（Text/Button/Image）是否严格绑定，否则应明硊错提?
