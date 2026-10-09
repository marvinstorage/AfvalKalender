#!/usr/bin/env bash
# Checks the docs: relative markdown links must resolve, and source paths named in backticks
# in docs/dev must exist. Run from anywhere; exits non-zero when something is broken.
set -u
cd "$(dirname "$0")/.."
fail=0

mapfile -t files < <(git ls-files '*.md' | grep -vE '^(openspec/changes/archive|\.agents)/')

for f in "${files[@]}"; do
  dir="$(dirname "$f")"
  # [text](target) links, skipping urls, anchors and mailto
  while IFS= read -r target; do
    target="${target%%#*}"
    [ -z "$target" ] && continue
    case "$target" in http*|mailto:*) continue ;; esac
    if [ ! -e "$dir/$target" ]; then
      echo "BROKEN LINK in $f: $target"
      fail=1
    fi
  done < <(grep -oE '\]\([^)]+\)' "$f" | sed -E 's/^\]\(//; s/\)$//; s/ .*$//')
done

# Source paths in docs/dev such as `AfvalKalender.Domain/Entities/Adres.cs`
for f in $(git ls-files 'docs/dev/*.md'); do
  while IFS= read -r p; do
    if [ ! -e "$p" ]; then
      echo "MISSING PATH in $f: $p"
      fail=1
    fi
  done < <(grep -oE '`AfvalKalender\.[A-Za-z.]+/[A-Za-z0-9_./-]+\.(cs|axaml|xaml|csproj)`' "$f" | tr -d '`' | sort -u)
done

[ "$fail" -eq 0 ] && echo "Docs OK"
exit "$fail"
