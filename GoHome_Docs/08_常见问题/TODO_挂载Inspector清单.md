# TODO：挂载Inspector清单（迁移后统一回归）

本页用于在“脚本目录迁移 / 生命周期分层调整”后，统一记录需要在 Unity Inspector 中确认的挂载项。

> 约定：本页是**可执行待办清单**，不写原理解释；原理请查 UI/架构相关章节。

## 1) 主菜单场景（MainMenuScene）

- **EventSystem**
  - 场景应存在一个可用的 `EventSystem`。
  - 若场景漏配，`MainMenuController` 会运行时创建兜底，但仍建议场景内显式配置（便于调试）。
- **MainMenuController**
  - 绑定：`mainPageRoot`、`confirmPageRoot`、`continueButton`、`confirmMessageText`、`feedbackText`

## 2) 游戏场景（GameScene）

### 2.1 Level 层（关卡流程对象）

确认场景中仅存在一份（避免重复单例报错）：  
`GameManager`、`ActionManager`、`AdvanceFlowController`、`BattleManager`、`EventManager`、`EnemyPoolService`、`RestManager`、`EndingManager`

### 2.2 Session 层（会话对象）

- `SessionRoot` 由代码运行时创建（`DontDestroyOnLoad`），一般不需要场景内手动摆放。
- 若你在场景中也手动挂了同名组件，请检查是否导致重复实例。

### 2.3 UI 层

- `UIManager`
  - 按 `UIManager` 脚本的 `[SerializeField]` 全量确认面板引用不为 `null`
- `HudStatusPanel`
  - 绑定：`PlayerDatePanel`、`RouteInfoPanel`
- `InGameSettingsController`
  - 绑定：`settingsPanelRoot`、`saveFailedDialogRoot`、`saveFailedMessageText`、`actionBarPanel`

## 3) 迁移后快速判定

- 若组件显示 `Missing (Mono Script)`：优先等待 Unity 完成 reimport；仍存在则需要手动重新挂载脚本。
- 若 UI 按钮无 Hover：优先检查 `EventSystem` 与 Canvas 的 raycast/遮罩遮挡。

