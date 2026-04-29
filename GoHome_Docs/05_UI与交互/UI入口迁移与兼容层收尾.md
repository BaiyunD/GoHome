# UI 入口迁移与兼容层收尾

本页用于统一 UI 入口口径：现有代码应走新入口，不再保留 legacy 入口调用。

## 名词定义（Page / Panel / Modal）

- `Page`：进入后会切换页面栈，影响常驻区显示；由 `UIStateMachine + UIRouter` 管理。
- `Panel`：普通面板开关，不进入页面栈，可与常驻区并存（是否独占由策略决定）。
- `Modal`：轻覆盖层，不进入 `PanelType`；由独立组件管理并通过 `UIManager` 显隐（如 `EventNarrationModal`、`ResultToastModal`）。

## 调用规范（必须遵守）

- 新代码必须使用：
  - `UIManager.OpenUIEntry(PanelType type)`
  - `UIManager.CloseUIEntry(PanelType type)`
- 约束：业务代码中不直接 `SetActive` 管理核心面板可见性，避免绕过 `UIRouter` 与状态机。

## Rest / End 的 flow-only 规则

- `PanelType.RestPage` 与 `PanelType.EndPage` 是 Page 路由下的 flow-only 页面。
- 仅允许通过 flow 方法打开：
  - `UIManager.OpenRestPanelFromFlow()`
  - `UIManager.OpenEndPanelFromFlow()`
- 原因：`UIManager.CanOpenPage()` 使用 gate 标记（`_allowRestOpenFromFlow` / `_allowEndOpenFromFlow`）拦截非流程打开，避免误入口污染页面栈。

## 路由行为说明（当前不变）

- 打开 Page 时，`UIRouter` 仍会调用 `HideEventNarrationText()` 主动隐藏 narration。
- 本轮仅做命名与文档同步，不改该路由行为。

## 回归清单（兼容层收尾）

- 按钮：行动按钮在事件/战斗/休息流程后按预期恢复，不会长期丢失。
- 事件：随机事件进出后，`RandomEvent` 与 narration 显示恢复正常。
- 战斗：进入战斗时打开 `Battle` 并关闭 `ActionBar`，结束后反向恢复。
- 休息：休息结算可正常展示与确认，UI 信息刷新正常。
- 页面栈：Page 打开/关闭顺序稳定；Rest/End 的 flow 调用不会误压页面栈。
