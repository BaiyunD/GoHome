# AdvanceFlow（前进/探索流程）

## 职责
- 统一扣除消耗（`energy -10`、`hunger -5`）
- 按概率分流：物资 / 随机事件 / 遇敌
- 状态机控制重复触发：`Idle / Running / Transit`
- 构建当前 `regionCode(main_sub)` 并执行严格校验/中断

## 权威数据源
- `AdvanceFlowController.State`

## 关键文件
- `Assets/Scripts/Manager/AdvanceFlowController.cs`
- 依赖：`RouteProgressManager`、`EventManager`、`BattleManager`、`EnemyPoolService`

## 常见修改路径
- 改分流概率：调整 `supplyProbability/randomEventProbability/enemyProbability`
- 改扣除时机/数值：看 `ExecutePrepareStage()` 的扣除逻辑
- 改“严格中断”策略：看 `HandleFlowFailure()` 与 `ExecuteFinalizeStage(...)`

## 风险与校验
- 高风险：Running/Transit 状态下仍允许重复触发（会导致多协程并发）
- 必查：子地区异常（<0）与 regionCode 校验失败时是否按预期中断并提示
