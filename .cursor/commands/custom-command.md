# custom-command

Write your command content here.

This command will be available in chat with /custom-command

/local-overview

Goal:
Read-only scan of ONLY the user-specified paths and produce a concise repo overview as a local-only Markdown note.

Hard constraints:
- Do NOT edit, format, rename, move, or delete any existing files/folders.
- Do NOT create files anywhere except the local-only notes folder.
- Do NOT include secrets (API keys, tokens, passwords, private URLs). If detected, redact and note “redacted”.
- Do NOT assume unstated paths. Only read the exact files/folders the user names.

Inputs (from user message each run):
- TARGET_PATHS: list of files/folders to scan (required)
- OUTPUT_FOLDER: default `.local_docs/` (can be overridden)
- OUTPUT_FILENAME: default `{TARGET_PATHS}-overview.md` (can be overridden)

Allowed actions:
1) Read directory trees under TARGET_PATHS.
2) Open and skim relevant files needed to understand purpose (README, package manifests, configs, entrypoints, main modules).
3) Summarize findings concisely.

Output:
- Create exactly one Markdown file at: `{OUTPUT_FOLDER}/{OUTPUT_FILENAME}`
- If OUTPUT_FOLDER does not exist, create it (only this folder).

Markdown structure (keep concise, 1-2 sentences per item max):
# Overview (generated locally)

## High-level purpose (3–7 bullets)
- …

## Key entrypoints / how it runs
- …

## Notable dependencies / configs

## Change Suggestion

