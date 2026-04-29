# regionCode 与 enemyId 规范与校验

本页把两个关键标识的“格式/含义/校验位置”统一下来，避免配置散乱与运行时难定位。

## 概念与边界

- `regionCode`：地区码，格式固定为 `main_sub`（例：`1_2`）
  - `main`：主地区编号（通常来自路线主地区）
  - `sub`：子地区编号（通常来自路线子地区）
- `enemyId`：敌人 ID，字符串标识，用于事件遇敌 → 战斗侧解析敌人模板

## 本项目约定

### 1) regionCode

- 格式必须满足：`<int>_<int>`
- 不能为空、不能有空格、不能缺少 `_`
- 校验工具：`EventCondition.ValidateRegionCodeOrThrow(regionCode, context)`
- 典型来源：
  - `AdvanceFlowController` 从路线状态构建当前地区码
  - `GameEvent.EventCondition` 作为事件筛选条件

### 2) enemyId

- 不能为空、不能空白字符
- 作为“跨系统联动”只允许值传递
  - 事件结果里写 `enemyId`
  - 战斗入口接收 `enemyId`
  - 敌人解析统一在 `EnemyPoolService`

## 常见错误与症状

- `regionCode` 写成 `1-2` 或 `1/2`：事件筛选/敌人池定位直接失败
- `enemyId` 漏填：事件遇敌后无法开战（应严格失败并给出错误）
- 地区池里没有对应 `enemyId`：开战失败，但如果错误不含上下文就很难定位

## 如何修改（入口/文件/方法）

- 改地区码构建规则：`Assets/Scripts/Manager/AdvanceFlowController.cs`
- 改事件条件字段：`Assets/Scripts/GameEvent.cs`（`EventCondition.regionCode`）
- 改敌人数据字段：`Assets/Scripts/EnemyData.cs`（`enemyId/regionCode`）
- 改敌人池解析：`Assets/Scripts/Manager/EnemyPoolService.cs`

## 验证清单

- 任意触发事件/遇敌前，`regionCode` 已通过严格校验（否则中断）
- 事件遇敌返回 `enemyId` 时：
  - `enemyId` 非空
  - 在当前 `regionCode` 的敌人池中能找到对应敌人
- 报错日志包含 `regionCode/enemyId`，可定位到“哪个配置缺失/哪个 ID 不存在”
