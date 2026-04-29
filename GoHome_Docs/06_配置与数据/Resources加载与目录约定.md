# Resources 加载与目录约定

本页把“哪些数据走 Resources、目录如何组织、改动后如何验证”讲清楚，避免出现“资源路径对不上但运行时静默失败”的问题。

## 概念与边界

- `Resources` 适合：少量、必须运行时按路径加载、且不想写 Addressables/AssetBundle 的数据。
- 不适合：大批量资源、需要按标签/异步/远程更新的内容（那应考虑 Addressables）。

本项目中，**地区数据**存在基于 `Resources/Regions` 的加载路径约定（由路线/地区系统消费）。

## 本项目约定

- 地区数据建议集中放在：
  - `Assets/Resources/Regions/`（或其子目录）
- 代码侧不要散落硬编码路径，优先集中在负责加载的 Manager 内部维护一个常量/字段（便于统一调整）。

## 常见错误与症状

- **路径不匹配**：运行时加载返回 `null`，但上层没做严格报错 → 后续出现空引用或地区为默认值。
- **文件名/目录名变更**：Unity 会维护 `.meta`，但 `Resources.Load("Regions/xxx")` 的路径不会自动更新。

## 如何修改（入口/文件/方法）

1. 先确认“谁在加载”
   - 通常在 `RouteProgressManager` 或地区相关 Manager 内部（检索 `Resources.Load` 与 `Regions` 关键字）。
2. 再按“最小改动”调整
   - 如果只是换目录：优先改加载路径常量/字段，不要改多处字符串。
3. 补齐严格校验
   - `Resources.Load` 返回 `null` 时，至少 `Debug.LogError` 输出“期望路径 + 当前 regionCode/地区信息”，避免静默。

## 验证清单

- 运行时能成功加载目标资源（不为 `null`）
- 切换主地区/子地区时，加载路径与 `regionCode` 对应关系正确
- 缺资源时能给出可定位的错误日志（包含路径与上下文）
