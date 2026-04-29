# Manager 目录冻结与命名落位规则

## 适用范围

本文档用于 GoHome 当前“先定规则、后迁移”的阶段，约束新增脚本命名与目录落位，避免 `Assets/Scripts/Manager/` 继续膨胀。

## 严格后缀命名（Strict Suffix）

新增核心脚本必须使用以下后缀之一：

- `*Root`：生命周期根容器（如 `AppRoot`、`SessionRoot`、`LevelRoot`）
- `*Service`：无场景状态的基础服务能力（如加载、存储、资源提供）
- `*Manager`：持有运行态并负责模块编排
- `*Controller`：UI 输入与界面交互控制

补充约束：

- 同一职责域内，不得并存语义重复的 `*Service` 和 `*Manager` 命名。
- 不使用泛化命名（如 `GameManager2`、`CoreHelper`）规避后缀规则。

## 目录落位规则（生命周期优先）

新增脚本放置顺序：

1. 先判定生命周期层：`App` / `Session` / `Level` / `UI`
2. 再判定功能子目录：如 `Root` / `Flow` / `Player` / `Progress` / `Combat` / `Events`
3. 最后检查后缀与职责是否一致

示例：

- 会话状态管理：`Assets/Scripts/Session/Player/*Manager.cs`
- 关卡流程编排：`Assets/Scripts/Level/Flow/*Manager.cs` 或 `*Controller.cs`
- 跨场景基础服务：`Assets/Scripts/App/Services/*Service.cs`
- UI 交互逻辑：`Assets/Scripts/UI/**/**/*Controller.cs`

## Manager 目录冻结规则

- 默认禁止新增：`Assets/Scripts/Manager/*.cs`
- 允许例外（必须满足其一）：
  - 迁移触达：为保障现有功能稳定，在迁移批次中对旧文件进行必要小改
  - 明确白名单：由架构规则或审查结论指定的临时保留点

例外改动必须在提交说明中标注：

- 例外原因（迁移触达/白名单）
- 预计迁移批次（App、Session、Level、UI 的哪一批）

## 迁移门禁（软 -> 硬）

- 软门禁（当前批次）：
  - 代码评审人工检查“新增脚本未进入 Manager 目录 + 命名后缀合规”
- 硬门禁（后续批次）：
  - 启用自动检查拦截新增 `Assets/Scripts/Manager/*.cs`（白名单除外）
  - 分批顺序：`App -> Session -> Level -> UI`，每批仅处理一个生命周期块
