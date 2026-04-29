# Battle（战斗系统）

## 职责
- ?结束战斗与回合推?- 维护战斗必（玩?敌人运时属性）
- 关战?UI，并在结束后恢行动入口
- 攌?`enemyId` 战（结合地区敌人池解析）

## 权威数据?- `BattleManager` 内部必：`_playerSnapshot/_enemySnapshot`
- 当前敌人模板：`_enemyTemplate`

## 关键文件
- `Assets/Scripts/Manager/BattleManager.cs`
- `Assets/Scripts/UIs/BattlePanel.cs`
- `Assets/Scripts/Manager/EnemyPoolService.cs`
- `Assets/Scripts/EnemyData.cs`

## 常俔跾
- 改战?UI 显示：从 `BattlePanel.RefreshView()` ?`BattleManager.GetXxxDisplay()` 入手
- 改敌人定位：?`StartBattleByEnemyId()` -> `EnemyPoolService.GetEnemyByIdInRegion()` 入手
- 改回合流程：?`PlayerNormalAttackFlow/EnemyNormalAttackFlow` 入手

## 风险与校?- 高险：战斗?结束时面板开关不对称，?ActionBar 状异?- 必查：`EnemyPoolService` 昐挂载且地区池配置完整，否则按严格失败跾世

