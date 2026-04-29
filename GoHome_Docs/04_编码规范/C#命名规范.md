# C# 命名规范

## 目标

统一命名以降低理解成本，避免同类概念出现多种命名风格导致误读。

## 命名规则（项目约定）

- **类名**：PascalCase，例如 `PlayerController`
- **方法名**：PascalCase，例如 `CalculateHealth`
  - Unity 生命周期方法保持 `Awake/Start/Update` 这种 PascalCase
- **私有字段**：`_camelCase`，例如 `_speed`
  - 可序列化字段用 `[SerializeField] private`，不要用 `public` 直接暴露
- **公有属性**：PascalCase，例如 `Health`
- **局部变量**：camelCase，例如 `targetPosition`
- **常量**：UPPER_SNAKE_CASE，例如 `MAX_PLAYER_COUNT`

## 常见修改场景

### 新增一个字段

1. 优先使用 `private` 字段
2. 需要 Inspector 可配时才加 `[SerializeField]`
3. 外部访问用属性或方法，不用直接 public 字段

### 新增一个配置型脚本字段

- 推荐：
  - `[SerializeField] private SomeConfig config;`
- 避免：
  - `public SomeConfig config;`（污染 API，且容易被其他脚本误用）

## 风险与校验

- **高风险**：字段命名与现有约定不一致，导致后续批量查找/重构困难。
- **必查**：新增可序列化字段后，是否需要在 Inspector 绑定（缺失会导致运行时报错或功能缺失）。
