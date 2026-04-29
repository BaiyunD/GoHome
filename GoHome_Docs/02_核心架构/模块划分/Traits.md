# Traits（特性系统）

## 职责
- 特定义注册持有特性理清?- 作为战斗/玩数的扩展位（触发、加成免疭?
## 权威数据?- `TraitManager` 的持有?-> 特集合（运时）
- Trait 定义资源（项盽前实现为脚本侧列衼

## 关键文件
- `Assets/Scripts/Manager/TraitManager.cs`
- `Assets/Scripts/UIs/TraitsPage.cs`

## 常俔跾
- 新特：先明硧发点（战?事件/行动），再在 `TraitManager` 增加注册与应用辑
- 改展示：?`TraitsPage.RefreshView()` ?trait 描述生成逻辑入手

## 风险与校?- 高险：及 UI 展示不加实际效果（或反过来），致看起来?实际没生效?- 必查：新游戏/结束游戏?trait 昐正确清空，避免跨泄漏

