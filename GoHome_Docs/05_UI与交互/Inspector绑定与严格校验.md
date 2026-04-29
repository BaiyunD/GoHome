# Inspector 绑定与严格校验（缺字段就报错）

本页约定 UI 面板脚本中 `[SerializeField]` 引用的绑定规则：**关键字段未绑定必须明确报错**，避免“静默空引用导致 UI 不刷新”。

## 你要改什么（常见目标）

- 新增 UI 文本/按钮/图片字段并要求必须绑定
- 修复某个面板不刷新，但没有报错的情况
- 为“配置缺失/绑定缺失”建立可定位的错误日志

## 规范（建议统一写法）

### 1) 字段定义

- 使用 `[SerializeField] private ...`，不要暴露 public 让外部随便改引用
- 字段名要表达用途：`enemyHpText`、`playerHpText`、`narrationText`

### 2) 刷新时严格校验

在 `RefreshView()`（或等价刷新方法）里：
- 若字段为 `null`：`Debug.LogError("PanelX.RefreshView -> xxx 未绑定...")`
- 若字段存在：才写入 `.text/.sprite/...`

原则：
- 错误日志必须包含：
  - **面板类名 + 方法名**
  - **缺失字段名**
  - **要求用户在 Inspector 绑定**

## 典型例子：BattlePanel 敌人 HP Text

- 文件：`Assets/Scripts/UI/InGame/Panels/BattlePanel.cs`
- 目标：新增并绑定 `enemyHpText`
- 刷新：使用 `BattleManager.GetEnemyHpDisplay()` 显示 `HP current/max`

必须验证：
- `enemyHpText` 未绑定时：有明确 `LogError`
- 绑定后：战斗开始/回合推进后，敌人 HP 能更新且格式正确

## 怎么验（必须验证项）

- 缺字段时，Console 报错信息能立刻定位到“哪个面板的哪个字段”
- 修复绑定后，UI 能恢复刷新（避免“报错修了但 UI 依旧不动”）

## TODO：挂载Inspector（UI迁移后必查）

> 本项目近期做过 UI 脚本目录迁移（`Assets/Scripts/UIs` -> `Assets/Scripts/UI/...`）。若 Unity 出现 `Missing (Mono Script)` 或 UI 不刷新，按以下清单逐项核对。

- `UIManager`（`Assets/Scripts/UI/Core/UIManager.cs`）
  - **必须绑定**：`HudStatusPanel`、`ActionBarPanel`、`EventNarrationModal`、`InventoryPage`、`CraftPage`、`BattlePanel`、`RestPage`、`EndPage`、`TraitsPage`、`CombatStatsPanel` 等（以脚本 `[SerializeField]` 为准）
- `HudStatusPanel`（`Assets/Scripts/UI/InGame/Hud/HudStatusPanel.cs`）
  - **必须绑定**：`playerStatusPanel`（`PlayerDatePanel`）、`routeStatusPanel`（`RouteInfoPanel`）
- `MainMenuController`（`Assets/Scripts/UI/Menu/MainMenuController.cs`）
  - **必须绑定**：`mainPageRoot`、`confirmPageRoot`、`continueButton`、`confirmMessageText`、`feedbackText`
- `InGameSettingsController`（`Assets/Scripts/UI/InGame/InGameSettingsController.cs`）
  - **必须绑定**：`settingsPanelRoot`、`saveFailedDialogRoot`、`saveFailedMessageText`、`actionBarPanel`
