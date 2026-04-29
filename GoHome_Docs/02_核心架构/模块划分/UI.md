# UI（UI管理）

## 职责
- 统一面板开关策略：
  - 持久 Panel（如 `HudStatus`、`ActionBar`）
  - Page（页面切换由页面栈管理）
  - 普通 Panel（允许与持久 Panel 同屏）
- 提供统一刷新入口：`UpdateInfo()`
- 协调事件/战斗等流程面板与行动按钮
- 管理轻覆盖层 Modal（`EventNarrationModal`、`ResultToastModal`）的展示协作

## 权威数据源
- `UIStateMachine`（持久集合、页面栈、激活集合）+ `UIRouter`（开关路由）

## 关键文件
- `Assets/Scripts/Manager/UIManager.cs`
- `Assets/Scripts/Manager/UIStateModel.cs`
- 典型组件：`Assets/Scripts/UIs/*Panel.cs`、`Assets/Scripts/UIs/*Page.cs`、`Assets/Scripts/UIs/*Modal.cs`

## 常见修改路径
- 新增面板：
  1. 增加 `PanelType`
  2. 在 `UIManager` 注册 panel 引用与开关逻辑
  3. 按需配置为 Page 或 Panel，并检查持久面板集合
- 改面板策略：优先只改 `UIPageMapping/UIStateMachine` 与集中控制路径，避免在业务侧到处 `SetActive`
- Modal 约束：`EventNarrationModal` 与 `ResultToastModal` 不进入 `PanelType`，也不进入 `UIStateMachine` modal 栈

## 风险与校验
- 高风险：业务侧自行隐藏/显示面板，绕开 UIManager（会导致状态难以追踪）
- 必查：面板引用是否在 Inspector 绑定完整；缺失必须明确报错
