# DEVLOG

## 2026-04-15

### 完成
- 将项目规则迁移到 `.cursor/rules/unity-csharp-guardrails.mdc`，后续按最小改动策略执行。
- 制作详情区改为显示“当前选中配方成品”的名称与描述，不再使用配方描述字段。
- 修复制作材料数量文本颜色残留问题（仅在缺料时红色）。
- 新增并接入共享结果提示：制作成功/失败与背包使用效果共用同一提示文本框。
- 制作页新增页码显示（`x/y`）。
- 制作配方改为自动从 `Resources/CraftRecipes` 读取并排序。
- 背包选中逻辑改为单选：默认选中第一个、唯一高亮、详情联动刷新。
- “使用”按钮与按钮文字按可用性联动显隐（仅可使用物品显示）。
- 背包使用后自动刷新列表，并尽量恢复原选中项。
- 修复“使用最后一个物品时结果提示显示异常”的时序问题（先缓存后扣除）。

### 命名与结构调整
- 新增 `SharedResultToast`，统一结果提示入口。
- 新增 `PanelDetailsPanel`，替换旧命名 `PaneletailsPanel`。
- 新增 `UITexts`，集中管理提示文案常量。
- 删除兼容壳文件：`PaneletailsPanel.cs`、`CraftResultToast.cs`。
- 删除不再使用的 `EffectPanel.cs` 及其引用。

### 主要修改文件
- `Assets/Scripts/Manager/CraftManager.cs`
- `Assets/Scripts/Manager/InventoryManager.cs`
- `Assets/Scripts/UIs/CraftPanel.cs`
- `Assets/Scripts/UIs/CraftDetailsPanel.cs`
- `Assets/Scripts/UIs/CraftRecipeSlotUI.cs`
- `Assets/Scripts/UIs/InventoryPanel.cs`
- `Assets/Scripts/UIs/PanelDetailsPanel.cs`
- `Assets/Scripts/UIs/SharedResultToast.cs`
- `Assets/Scripts/ItemFrame.cs`
- `Assets/Scripts/CraftRecipe.cs`
- `Assets/Scripts/UITexts.cs`

### 问题与风险
- 需确认场景中 `PanelDetailsPanel.useButton` 与 `useButtonText` 均已正确绑定。
- 需确认共享提示框对象挂在常驻层级，避免被某个独占面板一起隐藏。

### 下次计划
- 背包分类筛选（全部/材料/道具/食物）与多标签物品类型支持。
- 统一 UI 文案常量（包括按钮、提示、标题文案）。
- 梳理 Inspector 引用与场景对象命名，完成一轮清理。

## 2026-04-16

### 完成
- 物品类型从 `Common/Special` 重构为 `材料/食物/药品/道具/特殊物品`，并完成对应命名调整。
- 背包分类筛选落地：支持按标签过滤、支持“全部”分类，且数量为 `0` 的已获得物品仍会显示。
- “使用”库存为 `0` 的物品时，提示文案更新为 `你已经用完了>~<`。
- 事件系统按“地区优先 + 扩展判定”重构：默认先匹配地区，再执行扩展条件。
- 新增扩展判定基类与常用扩展：`EventConditionPredicate`、`EventDistancePredicate`、`EventDayPredicate`。
- 距离语义切换为“从 `0` 开始累计、每次前进 `+1`”，并同步回家结局判定。
- `GameEvent` 配置编辑体验优化：新增自定义 Inspector，字段显示“英文（中文）”。
- `EventResult` 新增结果类型开关（`Stat/Item`），Inspector 仅显示当前类型对应配置。
- 随机事件执行流程优化：按选项数量动态显示 `1/2` 个按钮；选项点击后统一隐藏按钮并关闭事件面板。
- 随机事件叙述文本从随机选项面板解耦为公用组件 `EventNarrationPanel`（与 `SharedResultToast` 职责分离）。
- `TODO.md` 新增待办：`EventItemCountPredicate`（按背包物品数量判定事件触发）。

### 命名与结构调整
- 新增 `EventNarrationPanel`，用于事件叙述文本展示（独立于随机选项按钮面板）。
- 删除 `RandomEventTextPanel`，统一命名与职责边界。
- 保留 `RandomEventPanel` 仅负责事件选项按钮显示与交互。

### 主要修改文件
- `Assets/Scripts/Item.cs`
- `Assets/Scripts/UIs/InventoryPanel.cs`
- `Assets/Scripts/UIs/PanelDetailsPanel.cs`
- `Assets/Scripts/UITexts.cs`
- `Assets/Scripts/GameConfig.cs`
- `Assets/Scripts/GameEvent.cs`
- `Assets/Scripts/Manager/GameManager.cs`
- `Assets/Scripts/UIs/ActionButtons.cs`
- `Assets/Scripts/Manager/EventManager.cs`
- `Assets/Scripts/Manager/UIManager.cs`
- `Assets/Scripts/Manager/EndingManager.cs`
- `Assets/Scripts/Manager/ActionManager.cs`
- `Assets/Scripts/UIs/RandomEventPanel.cs`
- `Assets/Scripts/UIs/EventNarrationPanel.cs`
- `Assets/Scripts/EventConditionPredicate.cs`
- `Assets/Scripts/EventDistancePredicate.cs`
- `Assets/Scripts/EventDayPredicate.cs`
- `Assets/Scripts/Editor/GameEventInspector.cs`
- `Assets/ScriptableObjects/EndingData.cs`
- `TODO.md`

### 问题与风险
- 需确认 `UIManager.eventNarrationPanel` 已在场景中正确绑定，否则事件叙述文本不会显示。
- 现有事件资产需按新结构复核（`EventResult.resultType`、`statResult/itemResult`、`location` 与 `extraPredicates` 配置）。
- 若后续改为严格地区触发，需评估“地区池为空”时的兜底策略是否保留。

### 下次计划
- 新增并接入 `EventItemCountPredicate`，支持基于背包物品数量的事件触发门槛。
- 梳理并批量校验随机事件资产配置（地区、扩展条件、结果类型与概率）。
- 视测试结果决定是否将随机事件筛选从“地区优先+全局兜底”切换为“严格地区匹配”。

## 2026-04-18

### 1) 物品体系切换到 `ItemDefinition`（SO）主链路
- 新增统一物品定义资产类型，基础信息改为从 `ItemDefinition` 驱动（`id/kind/displayName/description/icon/effectKey`）。
- `ItemRegistry` 改为从 `Resources/ItemDefinitions` 读取并构建运行时物品字典，不再依赖旧物品资产加载流程。
- 完成“纯新ID”清理：移除运行时 `LegacyId` 映射能力。
- 涉及：`Assets/Scripts/Items/ItemDefinition.cs`、`Assets/Scripts/Items/PrefabItems/ItemRegistry.cs`、`Assets/Scripts/Items/PrefabItems/DefinedItem.cs`

### 2) 背包分类与使用链路收敛
- 背包分类 Tab 收敛为单一“消耗品”入口，脚本侧去掉药品按钮字段与相关绑定逻辑。
- 使用按钮判定由旧类型判断切换为 `item.Kind == ItemKind.Consumable`，适配新物品体系。
- 临时使用行为改为“仅扣 1 个库存，不触发属性效果”，并统一提示文案。
- 涉及：`Assets/Scripts/UIs/InventoryPanel.cs`、`Assets/Scripts/UIs/PanelDetailsPanel.cs`、`Assets/Scripts/UITexts.cs`

### 3) 初始物品与ID链路调整
- 开局发放逻辑统一到新ID方案（当前按 `1..8` 发放基础材料各 30）。
- 清理 legacy 锚点发放逻辑，删除 `AddStartMaterialByLegacyId()`。
- 调试按键中的物品ID调用与新体系保持一致。
- 涉及：`Assets/Scripts/Manager/InventoryManager.cs`、`Assets/Scripts/Manager/GameManager.cs`

### 4) 编辑器工具与旧产物清理
- `ItemDefinitionTools` 精简为“分段自动分配ID”单一职责（001/101/201/301）。
- 删除旧迁移按钮与 `LegacyIdMap` 输出逻辑。
- 清理旧产物：删除 `Assets/Resources/ItemDefinitions/LegacyIdMap.json` 及其 `.meta`。
- 涉及：`Assets/Scripts/Editor/ItemDefinitionTools.cs`、`Assets/Resources/ItemDefinitions/LegacyIdMap.json`

## 当前状态（收尾）
- 新物品体系已跑通：`ItemDefinition -> ItemRegistry -> 背包显示/使用按钮判定`。
- 纯新ID主链路已建立，代码层旧 `LegacyId` 兼容逻辑已移除。
- 消耗品“使用”链路可用（扣库存+提示），材料/道具/特殊仍保持不可使用。
- `CraftRecipes` 与 `RandomEvents` 仍存在部分 `itemId/resultItemId/itemID=0` 的数据点，需后续手动校正。

- 下次继续：逐文件修正 `CraftRecipes` 的 `itemId/resultItemId`，清空 0 值占位。
- 下次继续：检查 `RandomEvents` 物品结果ID，补齐真实新ID或明确保留0。
- 下次继续：补齐 `icon_<id>.png` 并决定是否隐藏背包 0 数量物品格。

### 5) 今日补充（追加）
- 按新规则将今日报告改为“同日追加”写入，不再覆盖当日已存在内容。
- 纯新ID链路在物品/背包中继续验证：消耗品按钮按 `ItemKind.Consumable` 显示，材料/道具/特殊不显示。
- 消耗品“使用”行为临时定为：仅扣 1 个库存，不触发属性效果；提示文案改为“已使用1个xxx（暂未接入效果）”。
- 涉及：`Assets/Scripts/UIs/PanelDetailsPanel.cs`、`Assets/Scripts/UITexts.cs`、`.cursor/rules/daily-report-format.mdc`

## 2026-04-20

### 1) 特性系统基础落地
- 新增特性数据与枚举体系，明确“定义字典 + 持有者列表”的管理模型。
- 搭建特性效果基类与玩家特性触发基类，为后续逐条特性实现留出扩展位。
- 实现 `TraitManager`：定义注册、添加/删除、效果挂载与卸载、清空逻辑。
- 涉及：`Assets/Scripts/Data/TraitDefinition.cs`、`Assets/Scripts/Data/TraitDatabase.cs`、`Assets/Scripts/Traits/TraitEnums.cs`、`Assets/Scripts/Traits/TraitEffectBase.cs`、`Assets/Scripts/Traits/PlayerTraitEffectBase.cs`、`Assets/Scripts/Traits/TraitRuntimeContext.cs`、`Assets/Scripts/Manager/TraitManager.cs`

### 2) 特性 UI 接入与面板联动
- 新增特性面板脚本，支持按格式输出特性名称/描述，空字段显示 `null`。
- `UIManager` 新增 `PanelType.Traits` 独占面板分支，并接入显示/隐藏与刷新流程。
- `ActionButtons` 新增“特性”入口方法，保持与现有面板打开路径一致。
- 涉及：`Assets/Scripts/UIs/TraitPanel.cs`、`Assets/Scripts/Manager/UIManager.cs`、`Assets/Scripts/UIs/ActionButtons.cs`

### 3) 行动逻辑从 ActionButtons 向 ActionManager 迁移
- 将“前进/休息”核心玩法流程迁到 `ActionManager`，`ActionButtons` 保留 UI 入口职责。
- 在迁移过程中补充了单例缺失与重复挂载的错误提示，避免静默失败。
- 涉及：`Assets/Scripts/Manager/ActionManager.cs`、`Assets/Scripts/UIs/ActionButtons.cs`

### 4) 随机事件“点击后无后续”问题修复
- 确认根因是协程宿主生命周期：`ActionButtons` 面板隐藏后协程中断。
- 将前进事件流程协程宿主迁到常驻 `EventManager`，避免受 `ActionButtons` 显隐影响。
- 补充前进行动事件流程阶段日志（开始/随机结束/固定结束/总结束）便于排查。
- 涉及：`Assets/Scripts/Manager/EventManager.cs`、`Assets/Scripts/Manager/ActionManager.cs`、`Assets/Scripts/UIs/ActionButtons.cs`

### 5) ActionButtons 显隐策略调整
- 按最小改动将 `ActionButtons.Show/Hide` 改为整面板显隐，不再逐按钮控制。
- 保留了历史字段和辅助方法，避免一次性清理带来额外风险。
- 涉及：`Assets/Scripts/UIs/ActionButtons.cs`

## 当前状态（收尾）
- 特性系统、特性面板、ActionManager 迁移、随机事件协程宿主修复均已完成并通过基础 lint 检查。
- 当前随机事件按钮点击回调可到达，且后续流程不再因 `ActionButtons` 隐藏被中断。
- `ActionManager` 采用单例直接调用模式，未挂载时会有明确错误日志提示。
- 今日已将日报写入 `DEVLOG.md`。

- 下次继续建议：
- 在 Unity 场景中完整跑一轮“前进 -> 随机事件 -> 选项 -> 返回行动面板”并截图留档。
- 为事件结果增加更明确的玩家可见反馈（属性变化 toast 或结果停留确认）。
- 视需要清理 `ActionButtons` 中已闲置的按钮显隐辅助方法，做一次小型收敛重构。

### 6) 前进/探索控制器与遇敌链路重构（追加）
- 将前进逻辑从 `ActionManager` 拆出到独立流程控制器，建立 `Idle/Running/Transit` 状态机。
- 前进与探索统一改为“先扣消耗（能量-10、饥饿-5）再分流”，分流概率改为可配置 `60/20/20`。
- 删除前进流程中的固定事件调用路径；随机事件可通过结果回传 `enemyId` 进入战斗中转。
- 新增地区敌人池服务，战斗支持按 `enemyId + regionCode` 查找敌人并严格失败中断。
- 涉及：`Assets/Scripts/Manager/AdvanceFlowController.cs`、`Assets/Scripts/Manager/EnemyPoolService.cs`、`Assets/Scripts/Manager/ActionManager.cs`、`Assets/Scripts/Manager/EventManager.cs`、`Assets/Scripts/Manager/BattleManager.cs`

### 7) 事件/敌人数据结构收敛为严格配置（追加）
- 事件条件地区字段切换为 `regionCode(main_sub)`，并接入严格格式校验与异常中断。
- 事件结果新增 `EnemyEncounter` 与 `enemyId` 字段，事件中遇敌仅传字符串ID，不传敌人对象引用。
- `EnemyData` 新增 `enemyId` 与 `regionCode` 字段，配合地区池完成地区内敌人选择。
- `GameEvent` 自定义 Inspector 同步适配新字段展示与编辑路径。
- 涉及：`Assets/Scripts/GameEvent.cs`、`Assets/Scripts/EnemyData.cs`、`Assets/Scripts/Editor/GameEventInspector.cs`

### 8) 战斗面板改为非独占并新增敌方HP显示（追加）
- `UIManager` 中 `Battle` 与 `RandomEvent` 调整为非独占面板，不再触发独占栈隐藏常驻面板。
- 战斗开始保持“打开战斗面板 + 关闭行动按钮”，战斗结束恢复行动按钮。
- `BattlePanel` 新增 `enemyHpText`，实时显示敌方 `HP当前/最大`；未绑定时严格输出错误日志。
- `BattleManager` 新增 `GetEnemyHpDisplay()` 供面板刷新调用。
- 涉及：`Assets/Scripts/Manager/UIManager.cs`、`Assets/Scripts/UIs/BattlePanel.cs`、`Assets/Scripts/Manager/BattleManager.cs`

### 9) 工作流规则沉淀（追加）
- 新增“实现请求默认在 new agent 执行”的项目规则，并补充统一回报结构要求（改动/验证/Inspector待配）。
- 规划规则补充“执行前确认是否使用 new agent”与“计划中包含可验收条目”。
- 今日与本次会话确认链路已形成：多轮确认 -> 计划 -> new agent 实施 -> 主线程复核 -> 汇报。
- 涉及：`.cursor/rules/implementation-default-new-agent.mdc`、`.cursor/rules/plan-confirmation-style.mdc`

## 当前状态（收尾）
- 前进/探索分流、事件中转遇敌、战斗敌人ID定位、面板非独占策略已完成代码落地。
- 关键改动文件 `ReadLints` 检查通过；`dotnet build` 仍受项目内既有缺失脚本阻塞（非本次新增）。
- 战斗面板敌方HP显示功能已接入，需在 Inspector 绑定 `enemyHpText`。
- 今日已按规则将报告追加写入 `DEVLOG.md`。

- 下次继续建议：
- 在 Unity 场景中做端到端回归：前进/探索 -> 随机事件 -> 中转遇敌 -> 战斗结束回 Idle。
- 批量补齐 `EnemyData` 与 `GameEvent` 的 `regionCode/enemyId` 配置并做一致性检查。
- 为严格中断路径补充更友好的玩家提示文案与错误分类日志。

## 2026-04-22

### 完成
- 完成“使用手册同步”两批更新：先修核心流程口径，再统一架构/排错文档口径。
- UI 文档统一为单入口：公共调用以 `OpenUIEntry/CloseUIEntry` 为准，不再保留 legacy 公共入口口径。
- UI 分层文档统一为“当前 Page/Panel，Modal 预留”，并补充 `UIRouter + UIStateMachine` 路由/状态说明。
- `UpdateInfo` 文档改为“有限刷新”真实行为：刷新 HUD/Trait，并收起事件叙述。
- 地区与前进流程文档改为分阶段管线现状（Prepare/AdvanceDistance/SelectFeedback/Execute/Finalize），并修正失败处理入口说明。
- 事件遇敌文档补齐当前行为：`enemyId` 为空时跳过开战（当前非 hard fail），同时将 hard fail 前移保留为改造项。
- 完成知识库入口同步：项目根 `README`、`GoHome_Docs/README`、`02_核心架构/README`、`05_UI与交互/README` 统一“当前实现优先”阅读口径。
- 新增“今日变更摘要与待实现项”文档，并挂到 `GoHome_Docs` 首页导航。

### 主要修改文件
- `README.md`
- `GoHome_Docs/README.md`
- `GoHome_Docs/02_核心架构/README.md`
- `GoHome_Docs/02_核心架构/整体设计.md`
- `GoHome_Docs/02_核心架构/模块划分.md`
- `GoHome_Docs/02_核心架构/模块划分/UI.md`
- `GoHome_Docs/02_核心架构/模块划分/AdvanceFlow.md`
- `GoHome_Docs/03_功能模块/行动与前进探索.md`
- `GoHome_Docs/03_功能模块/随机事件与遇敌中转.md`
- `GoHome_Docs/05_UI与交互/README.md`
- `GoHome_Docs/05_UI与交互/面板管理与独占策略.md`
- `GoHome_Docs/05_UI与交互/信息刷新链路UpdateInfo.md`
- `GoHome_Docs/07_排错与调试/严格校验的定位与前移.md`
- `GoHome_Docs/08_常见问题/严格校验报错对照与处理.md`
- `GoHome_Docs/09_变更记录/2026-04-22_手册同步与重构收尾.md`

### 问题与风险
- 目前手册口径已同步到“当前实现优先”，但部分“规划项”尚未代码落地（例如更严格的 enemyId 空值 hard fail、更前置的流程校验）。
- 仓库状态无法通过 git 直接核对（当前工作目录未识别为 git 仓库），本次以文件落地结果为准。

### 下次计划
- 按“待实现项”优先级推进代码落地，并在完成后同步回写对应章节的“当前实现”段落。
- 补一轮最小回归记录（新开游戏、HUD/属性、前进/探索、事件遇敌）并追加到 `DEVLOG.md`。
