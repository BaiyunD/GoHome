# Action（动入口）

## 职责
- 对提供“前?探索/休息”的调用入口（面?UI?- 前进/探索委托?`AdvanceFlowController`
- 休息流程委托?`RestManager`

## 权威数据?- 木块自躸持有权威状，主做入口转发与少量 UI 提示

## 关键文件
- `Assets/Scripts/Manager/ActionManager.cs`
- `Assets/Scripts/UIs/ActionBar.cs`
- `Assets/Scripts/Manager/RestManager.cs`

## 常俔跾
- 新动按钼
  1. ?`ActionBar` 增加 `OnXxx()` 入口
  2. ?`ActionManager` 增加 `TryXxx()` 方法
  3. ?UI 绑定按钮?`OnXxx()`

## 风险与校?- 高险：?`ActionBar` 里实现完整业务辑（面板隐藏会导致协程/状不収?- 必查：`AdvanceFlowController`/`RestManager` 等单例是否在场景且挂载

