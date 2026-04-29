# Event（事件系统）

## 职责
- 基于地区码筛选随机事?- 驱动事件 UI：叙述文?+ 选项按钮
- 执事件结果：属?物品/遇敌丽（`enemyId` 值传递）

## 权威数据?- `EventManager.randomEvents`（事件池?
## 关键文件
- `Assets/Scripts/Manager/EventManager.cs`
- `Assets/Scripts/GameEvent.cs`
- `Assets/Scripts/UIs/RandomEventPanel.cs`
- `Assets/Scripts/UIs/EventNarrationModal.cs`

## 常俔跾
- 改事件筛选条件：?`GetRandomEventByRegionCode()` ?`GameEvent.EventCondition` 入手
- 新结果类型：扩?`EventResultType` ?`ApplyEventResult()` 的分?- 事件丽遇敌：确保只?`enemyId`，由战斗侧按地区池解析敌?
## 风险与校?- 高险：事件面板关与行动按钮关顺序不致?UI 状错?- 必查：事件资源配?`regionCode/enemyId` 昐完整，否则会严格失败世

