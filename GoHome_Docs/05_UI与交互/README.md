# 05_UI与交互

## 本章边界

本章只回答“**UI 怎么管**”与“**交互怎么接流程**”：

- 面板开关策略（Page/Panel 分层，Modal 轻覆盖层）
- 信息刷新链路（`UIManager.UpdateInfo()` 谁来调、什么时候调）
- Inspector 绑定与严格校验（缺字段必须明确报错）

> 业务功能如何改，请回到：[`../03_功能模块/README.md`](../03_功能模块/README.md)

## 推荐阅读顺序（规划阶段）

1. 先读 [面板管理与独占策略](./面板管理与独占策略.md)
2. 再读 [信息刷新链路UpdateInfo](./信息刷新链路UpdateInfo.md)
3. 再读 [UI入口迁移与兼容层收尾](./UI入口迁移与兼容层收尾.md)
4. 最后读 [Inspector绑定与严格校验](./Inspector绑定与严格校验.md)

## 索引（细节页）

- [面板管理与独占策略](./面板管理与独占策略.md)
- [信息刷新链路UpdateInfo](./信息刷新链路UpdateInfo.md)
- [UI入口迁移与兼容层收尾](./UI入口迁移与兼容层收尾.md)
- [Inspector绑定与严格校验](./Inspector绑定与严格校验.md)
- [面板管理与独占策略](./面板管理与独占策略.md)：包含 `EventNarrationModal` / `ResultToastModal` 的非 PanelType 归类约束
