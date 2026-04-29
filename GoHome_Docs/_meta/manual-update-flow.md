# 手册自动更新应用策略

## 触发方式

- 仅手动触发：
  - 生成草稿：`python tools/docs/update_manual_draft.py draft ...`
  - 应用草稿：`python tools/docs/update_manual_draft.py apply ...`

## 草稿生成规则

- 输入来源：
  - 默认：`git diff --name-only`（工作区变更）
  - 可选：指定 `--source-range HEAD~1..HEAD`
  - 或直接传 `--files`
- 映射依据：`GoHome_Docs/_meta/doc-map.json`
- 输出位置：`GoHome_Docs/_drafts/`

## 确认后落盘规则（增量、低风险）

- 应用时只“追加”到目标手册页，不重写整页。
- 每次应用会在目标文件末尾追加：
  - `## 自动同步草稿（时间）`
  - 来源草稿路径
  - 对应 `### TARGET: ...` 草稿块内容
- 若目标文件不存在：跳过并输出 warning，不中断其他目标。

## 部分接受策略

- 可使用 `--accept-targets` 指定目标文件（逗号分隔），实现部分落盘。
- 未被选择的目标保持在草稿中，不会改正式手册。

## DEVLOG 记录策略

- 每次成功应用后，在 `DEVLOG.md` 末尾追加一段：
  - 日期
  - 草稿文件路径
  - 落盘目标文件列表
- 记录目标是“可追溯来源”，不是替代手册正文。

## 冲突与后续改进

- 当前版本采用 append-only，规避自动覆盖冲突。
- 如果后续需要“写入指定章节而非末尾追加”，建议引入锚点策略：
  - 先定位固定标题（如“如何修改”）
  - 定位失败则降级为末尾追加
