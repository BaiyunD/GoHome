# RegionLootTable 配置与命名规范

本页用于说明前进/探索物资分支的数据配置方式，目标是：配置可读、校验可定位、规则与代码一致。

## 关联实现（代码落点）

- 配置定义：`Assets/Scripts/Data/RegionLootTable.cs`
- 掉落执行：`Assets/Scripts/Manager/RegionLootService.cs`
- 流程接入：`Assets/Scripts/Manager/AdvanceFlowController.cs`

## 字段说明（RegionLootTable）

- `noLootProbability`：不掉落（0 种）概率，默认 `20`
- `oneLootProbability`：掉落 1 种概率，默认 `50`
- `twoLootProbability`：掉落 2 种概率，默认 `20`
- `threeLootProbability`：掉落 3 种概率，默认 `10`
- `xiaoYueBraceletItemId`：晓月手链物品 ID
- `retryChancePerLevel`：每级触发“空结果重试一次”的概率（0~1）
- `regionPools`：按地区编码配置条目池

说明：
- 代码会按四档概率先决定本次“掉落种数（0/1/2/3）”，再从地区池抽条目。
- 若地区池缺失、条目无效、ID 非法，会触发严格中断并回到 `Idle`。

## 地区池规则（RegionLootPool）

- `regionCode` 格式必须为 `main_sub`（数字_数字），例如 `1_2`
- 必须与运行时地区编码完全一致（来源于 `BuildCurrentRegionCode()`）
- 每个地区池应至少有一个有效条目

## 条目规则（LootEntry）

- `rewardType = Item`：
  - `itemId > 0`
  - `weight > 0`
  - `countOption = One/Two`
- `rewardType = Money`：
  - `moneyAmount > 0`
  - `weight > 0`
  - `countOption = One/Two`（金额会乘以数量）

抽取规则：
- 单次抽取不重复同一条目
- 数量由 `countOption` 决定：`One=1`、`Two=2`

## 命名规则（示例配置）

- 资产命名：`RegionLootTable_{用途}`
  - 例：`RegionLootTable_Main`
- 地区编码命名：严格 `main_sub`
  - 例：`0_0`、`1_0`、`1_1`
- 条目可读标识建议（便于人工核对）：
  - 物品：`Item:{itemId}:{名称}`
  - 金钱：`Money:{amount}`

## 最小可运行配置骨架（示例）

- 表头：
  - `noLootProbability=20`
  - `oneLootProbability=50`
  - `twoLootProbability=20`
  - `threeLootProbability=10`
- `regionPools` 至少包含当前会进入的地区编码
- 每个地区池至少一个有效条目（`weight>0` 且 ID/金额合法）

## 开发侧验证清单

- `AdvanceFlowController` 已挂载 `regionLootTable`
- 命中物资分支时，背包物品或金钱真实变化
- 空结果旁白与掉落旁白都可见
- 晓月手链仅在空结果时触发，且最多重试一次
- 异常配置时日志可定位（包含地区编码/字段语义）
