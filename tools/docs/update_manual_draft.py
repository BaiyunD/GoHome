#!/usr/bin/env python3
"""
Generate/apply handbook update drafts.

Design goals:
- Manual trigger only
- Draft first, apply after confirmation
- Append-only updates for low risk
"""

from __future__ import annotations

import argparse
import datetime as dt
import json
import re
import subprocess
import sys
from collections import defaultdict
from pathlib import Path
from typing import Dict, List, Tuple


ROOT = Path(__file__).resolve().parents[2]
DEFAULT_MAP = ROOT / "GoHome_Docs" / "_meta" / "doc-map.json"
DEFAULT_TEMPLATE = ROOT / "GoHome_Docs" / "_meta" / "draft-template.md"
DEFAULT_DRAFT_DIR = ROOT / "GoHome_Docs" / "_drafts"
DEFAULT_DEVLOG = ROOT / "DEVLOG.md"


def run_git_diff(repo_root: Path, source_range: str) -> List[str]:
    cmd = ["git", "diff", "--name-only"]
    if source_range and source_range != "working-tree":
        cmd.append(source_range)
    try:
        out = subprocess.check_output(cmd, cwd=repo_root, text=True, stderr=subprocess.STDOUT)
    except subprocess.CalledProcessError as exc:
        print(f"[warn] git diff failed, fallback to empty set: {exc.output.strip()}", file=sys.stderr)
        return []
    return [line.strip().replace("\\", "/") for line in out.splitlines() if line.strip()]


def load_json(path: Path) -> dict:
    return json.loads(path.read_text(encoding="utf-8"))


def load_text(path: Path) -> str:
    return path.read_text(encoding="utf-8")


def rule_matches(rule: dict, file_path: str) -> bool:
    match_type = rule.get("matchType", "prefix")
    pattern = str(rule.get("match", "")).replace("\\", "/")
    if match_type == "prefix":
        return file_path.startswith(pattern)
    if match_type == "exact":
        return file_path == pattern
    if match_type == "glob":
        regex = "^" + re.escape(pattern).replace(r"\*", ".*") + "$"
        return re.match(regex, file_path) is not None
    return False


def compute_targets(changed_files: List[str], mapping: dict) -> Tuple[Dict[str, dict], List[str]]:
    target_map: Dict[str, dict] = {}
    unmapped: List[str] = []
    rules = sorted(mapping.get("rules", []), key=lambda x: int(x.get("priority", 0)), reverse=True)

    for file_path in changed_files:
        matched = [r for r in rules if rule_matches(r, file_path)]
        if not matched:
            unmapped.append(file_path)
            continue
        for rule in matched:
            for target in rule.get("targets", []):
                if target not in target_map:
                    target_map[target] = {"files": set(), "hints": set(), "rules": set()}
                target_map[target]["files"].add(file_path)
                for hint in rule.get("sectionHint", []):
                    target_map[target]["hints"].add(hint)
                target_map[target]["rules"].add(rule.get("name", "unnamed-rule"))
    return target_map, unmapped


def build_target_overview(target_map: Dict[str, dict]) -> str:
    if not target_map:
        return "- 无命中目标。"
    lines = []
    for target, meta in sorted(target_map.items()):
        files = ", ".join(sorted(meta["files"]))
        lines.append(f"- `{target}`")
        lines.append(f"  - 触发文件：{files}")
    return "\n".join(lines)


def build_proposed_updates(target_map: Dict[str, dict]) -> str:
    if not target_map:
        return "（无）"
    chunks = []
    for target, meta in sorted(target_map.items()):
        hints = "、".join(sorted(meta["hints"])) if meta["hints"] else "如何修改"
        files = "，".join(sorted(meta["files"]))
        chunks.append(
            "\n".join(
                [
                    f"### TARGET: {target}",
                    f"- 触发来源：{files}",
                    f"- 建议更新章节：{hints}",
                    "- 建议追加草稿：",
                    "  - 概念与边界：补充本次改动影响范围。",
                    "  - 本项目约定：补充新增约束与默认策略。",
                    "  - 如何修改：给出入口文件与方法。",
                    "  - 验证清单：补充最小回归项。",
                    "",
                ]
            )
        )
    return "\n".join(chunks).strip()


def render_template(template: str, values: dict) -> str:
    out = template
    for key, value in values.items():
        out = out.replace("{{" + key + "}}", value)
    return out


def slugify_topic(topic: str) -> str:
    topic = topic.strip().replace(" ", "_")
    topic = re.sub(r"[^\w\-\u4e00-\u9fff]+", "_", topic)
    return topic or "manual_update"


def generate_draft(args: argparse.Namespace) -> int:
    repo_root = Path(args.repo_root).resolve()
    map_path = Path(args.map_path).resolve()
    template_path = Path(args.template_path).resolve()
    draft_dir = Path(args.draft_dir).resolve()
    draft_dir.mkdir(parents=True, exist_ok=True)

    changed_files = [f.replace("\\", "/") for f in args.files] if args.files else run_git_diff(repo_root, args.source_range)
    mapping = load_json(map_path)
    template = load_text(template_path)

    target_map, unmapped = compute_targets(changed_files, mapping)

    now = dt.datetime.now()
    stamp = now.strftime("%Y-%m-%d-%H%M")
    topic = slugify_topic(args.topic or "manual_update")
    draft_name = f"{stamp}_{topic}.md"
    draft_path = draft_dir / draft_name

    target_overview = build_target_overview(target_map)
    proposed_updates = build_proposed_updates(target_map)
    unmapped_text = "\n".join([f"- `{f}`" for f in unmapped]) if unmapped else "- 无"
    recommendation = "优先处理命中最多变更文件的前 1-2 个目标页。"

    values = {
        "generated_at": now.strftime("%Y-%m-%d %H:%M:%S"),
        "source_range": args.source_range,
        "draft_file": str(draft_path.relative_to(repo_root)).replace("\\", "/"),
        "change_summary": "\n".join([f"- `{f}`" for f in changed_files]) if changed_files else "- 未检测到变更文件",
        "target_overview": target_overview,
        "proposed_updates": proposed_updates,
        "unmapped_changes": unmapped_text,
        "recommendation": recommendation,
        "applied_targets": "（待确认）",
        "apply_note": "（待确认）",
    }
    content = render_template(template, values)
    draft_path.write_text(content + "\n", encoding="utf-8")

    print(f"[ok] draft generated: {draft_path}")
    print(f"[info] targets: {len(target_map)}, unmapped: {len(unmapped)}")
    return 0


def parse_target_blocks(text: str) -> Dict[str, str]:
    lines = text.splitlines()
    blocks: Dict[str, List[str]] = {}
    current_target = None
    current_lines: List[str] = []
    for line in lines:
        if line.startswith("### TARGET: "):
            if current_target:
                blocks[current_target] = current_lines[:]
            current_target = line.replace("### TARGET: ", "", 1).strip()
            current_lines = [line]
        elif current_target:
            current_lines.append(line)
    if current_target:
        blocks[current_target] = current_lines
    return {k: "\n".join(v).strip() for k, v in blocks.items()}


def append_to_target(repo_root: Path, target_rel: str, block_text: str, draft_rel: str) -> bool:
    target = (repo_root / target_rel).resolve()
    if not target.exists():
        print(f"[warn] skip missing target: {target_rel}")
        return False
    timestamp = dt.datetime.now().strftime("%Y-%m-%d %H:%M")
    append_text = (
        f"\n\n## 自动同步草稿（{timestamp}）\n"
        f"- 来源草稿：`{draft_rel}`\n"
        f"{block_text}\n"
    )
    target.write_text(target.read_text(encoding="utf-8") + append_text, encoding="utf-8")
    return True


def append_devlog(devlog_path: Path, draft_rel: str, applied: List[str]) -> None:
    today = dt.datetime.now().strftime("%Y-%m-%d")
    lines = [
        "",
        f"### 手册自动同步（{today}）",
        f"- 草稿：`{draft_rel}`",
        "- 落盘目标：",
    ]
    lines.extend([f"  - `{t}`" for t in applied])
    devlog_path.write_text(devlog_path.read_text(encoding="utf-8") + "\n".join(lines) + "\n", encoding="utf-8")


def apply_draft(args: argparse.Namespace) -> int:
    repo_root = Path(args.repo_root).resolve()
    devlog_path = Path(args.devlog_path).resolve()
    if args.latest:
        draft_candidates = sorted(Path(args.draft_dir).resolve().glob("*.md"), key=lambda p: p.stat().st_mtime, reverse=True)
        if not draft_candidates:
            print("[err] no draft files found", file=sys.stderr)
            return 2
        draft_path = draft_candidates[0]
    else:
        if not args.draft:
            print("[err] --draft is required unless --latest is provided", file=sys.stderr)
            return 2
        draft_path = Path(args.draft).resolve()

    if not draft_path.exists():
        print(f"[err] draft not found: {draft_path}", file=sys.stderr)
        return 2

    draft_text = draft_path.read_text(encoding="utf-8")
    target_blocks = parse_target_blocks(draft_text)
    if not target_blocks:
        print("[info] no target blocks in draft, nothing to apply")
        return 0

    selected = set(target_blocks.keys())
    if args.accept_targets:
        selected = set([s.strip() for s in args.accept_targets.split(",") if s.strip()])

    applied: List[str] = []
    draft_rel = str(draft_path.relative_to(repo_root)).replace("\\", "/")
    for target, block in target_blocks.items():
        if target not in selected:
            continue
        if append_to_target(repo_root, target, block, draft_rel):
            applied.append(target)

    if not applied:
        print("[warn] no targets applied")
        return 1

    append_devlog(devlog_path, draft_rel, applied)
    print(f"[ok] applied targets: {len(applied)}")
    for item in applied:
        print(f" - {item}")
    return 0


def build_parser() -> argparse.ArgumentParser:
    p = argparse.ArgumentParser(description="Generate/apply handbook update drafts.")
    sub = p.add_subparsers(dest="command", required=True)

    pd = sub.add_parser("draft", help="Generate draft only.")
    pd.add_argument("--repo-root", default=str(ROOT))
    pd.add_argument("--map-path", default=str(DEFAULT_MAP))
    pd.add_argument("--template-path", default=str(DEFAULT_TEMPLATE))
    pd.add_argument("--draft-dir", default=str(DEFAULT_DRAFT_DIR))
    pd.add_argument("--source-range", default="working-tree", help="git range (e.g. HEAD~1..HEAD)")
    pd.add_argument("--topic", default="manual_update")
    pd.add_argument("--files", nargs="*", default=[], help="explicit changed files (skip git diff)")

    pa = sub.add_parser("apply", help="Apply selected targets from a draft.")
    pa.add_argument("--repo-root", default=str(ROOT))
    pa.add_argument("--draft-dir", default=str(DEFAULT_DRAFT_DIR))
    pa.add_argument("--devlog-path", default=str(DEFAULT_DEVLOG))
    pa.add_argument("--draft", default="")
    pa.add_argument("--latest", action="store_true", help="apply latest draft file in draft dir")
    pa.add_argument("--accept-targets", default="", help="comma-separated target files to apply")

    return p


def main() -> int:
    args = build_parser().parse_args()
    if args.command == "draft":
        return generate_draft(args)
    if args.command == "apply":
        return apply_draft(args)
    return 2


if __name__ == "__main__":
    raise SystemExit(main())
