# 06_配置与数据

## 本章边界

本章只描述“数据怎么组织、怎么配置、怎么校验”：

- `Resources` 加载与目录约定（哪些数据走 Resources）
- `ScriptableObject` 的使用规范（哪些字段必须填、如何扩展）
- `regionCode(main_sub)` 与 `enemyId` 的命名与严格校验规则
- 事件与敌人数据：**编辑器如何配** + **代码侧约束与扩展**
- 物资掉落配置：`RegionLootTable` 字段、命名与校验约束

> 业务功能入口请优先从：[`../03_功能模块/README.md`](../03_功能模块/README.md)

## 推荐阅读顺序（规划阶段）

1. 先读本页索引（只点你这次要改的配置）
2. 再读对应细节页（避免一次性读完）

## 索引（细节页）

- [Resources加载与目录约定](./Resources加载与目录约定.md)
- [ScriptableObject配置规范](./ScriptableObject配置规范.md)
- [RegionLootTable配置与命名规范](./RegionLootTable配置与命名规范.md)
- [道具效果资产化迁移规范](./道具效果资产化迁移规范.md)
- [道具多效果挂载规范](./道具多效果挂载规范.md)
- [道具效果分发顺序与冲突规则](./道具效果分发顺序与冲突规则.md)
- [regionCode与enemyId规范与校验](./regionCode与enemyId规范与校验.md)
- [事件与敌人数据（编辑器配置）](./事件与敌人数据_编辑器配置.md)
- [事件与敌人数据（代码约束与扩展）](./事件与敌人数据_代码约束与扩展.md)
