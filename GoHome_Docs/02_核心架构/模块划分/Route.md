# Route（路线进度）

## 职责
- 维护 `day` / `distance` 与当前主/子地区
- distance 变化时自动切换主地区
- 广播进度变化事件供 UI/系统消费

## 权威数据源
- `RouteProgressManager` 内部字段：
  - `_day` / `_distance`
  - `_currentMainRegion` / `_currentSubRegionId`

## 关键文件
- `Assets/Scripts/Manager/RouteProgressManager.cs`
- `Assets/Scripts/MainRegionData.cs`
- `Assets/Scripts/UIs/RouteInfoPanel.cs`

## 对外事件/API
- `OnDayChanged(int)`
- `OnDistanceChanged(int)`
- `OnMainRegionChanged(MainRegionData)`
- `Initialize(startDistance, startDay)` / `Advance(delta)` / `Rest(dayDelta)`

## 常见修改路径
- 改主地区切换规则：从 `HandleDistanceChanged()` 与 `SetCurrentMainRegion()` 入手
- 改子地区来源/语义：从 `GetCurrentSubRegionId()` 与初始化逻辑入手
- 改 UI 展示：在 `RouteInfoPanel.UpdateInfo()` 或 UIManager 刷新链路中处理

## 风险与校验
- 高风险：在其他系统里“私自维护”distance/day（会与权威源冲突）
- 必查：地区资源是否按 `Resources/Regions` 正确加载（`resourcesFolder`）
