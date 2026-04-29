# update_manual_draft 使用说明

## 1) 生成草稿（不改正式手册）

```bash
python tools/docs/update_manual_draft.py draft --topic "battle_fix"
```

常用参数：

- `--source-range HEAD~1..HEAD`：按提交区间生成草稿
- `--files <file1> <file2> ...`：直接指定变更文件
- `--topic xxx`：草稿文件名后缀

## 2) 应用草稿（确认后落盘）

```bash
python tools/docs/update_manual_draft.py apply --latest
```

部分接受（仅应用指定目标页）：

```bash
python tools/docs/update_manual_draft.py apply --latest --accept-targets "GoHome_Docs/03_功能模块/战斗与敌人数据.md,GoHome_Docs/08_常见问题/战斗与事件常见异常.md"
```

## 3) 数据来源

- 映射规则：`GoHome_Docs/_meta/doc-map.json`
- 草稿模板：`GoHome_Docs/_meta/draft-template.md`
- 应用策略：`GoHome_Docs/_meta/manual-update-flow.md`
