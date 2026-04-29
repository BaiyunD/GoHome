# Game（生?结局?
## 职责
- 持有玩运时信恼`PlayerInfo`?- 订阅跺进度变化并进行关锈定（到/时间/健康归零等）
- 对发出“新游戏/游戏结束”事?
## 权威数据?- `GameManager.playerInfo`
- `GameManager.Config`（`GameConfig`?
## 关键文件
- `Assets/Scripts/Manager/GameManager.cs`
- `Assets/Scripts/Manager/EndingManager.cs`
- `Assets/Scripts/UIs/EndPage.cs`

## 对事件/API
- `NewGameEvent`：新游戏初化完成后通知 UI
- `GameOverEvent`：游戏结束知 UI/结局系统
- `NewGame()` / `GameOver()`

## 常俔跾
- 调整“到家结束：?`GameManager.CheckDistance()` ?`HomeDistance`
- 调整“健康归零结束：?`CheckHealth()` 订阅与判定条?- 调整“时间压力：?`CheckDay()`（当前主要日志提示）

## 风险与校?- 高险：多个地方重判定 GameOver 导致多触发 UI 切换
- 必查：`playerInfo.health.OnChangedEvent` 昐重绑定/解绑（避免泄漏或重回调?
