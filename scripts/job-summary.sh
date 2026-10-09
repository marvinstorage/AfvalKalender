#!/usr/bin/env bash
# Writes code statistics and unit test line coverage to the GitHub job summary
# (or to stdout when run locally). cloc is optional.
set -u
cd "$(dirname "$0")/.."
out="${GITHUB_STEP_SUMMARY:-/dev/stdout}"

{
  echo "## Code statistics"
  if command -v cloc >/dev/null 2>&1; then
    cloc --vcs=git --md --quiet 2>/dev/null | sed '/^cloc|/d'
  else
    echo "cloc is not installed"
  fi

  echo
  echo "## Unit test coverage (lines)"
  echo
  echo "| Test project | Line coverage |"
  echo "|---|---:|"
  for dir in unit infrastructure desktop; do
    report="$(find "coverage/$dir" -name coverage.cobertura.xml 2>/dev/null | head -n 1)"
    if [ -n "$report" ]; then
      rate="$(grep -oE 'line-rate="[0-9.]+"' "$report" | head -n 1 | grep -oE '[0-9.]+')"
      echo "| $dir | $(awk -v r="$rate" 'BEGIN{printf "%.1f%%", r*100}') |"
    else
      echo "| $dir | no report |"
    fi
  done
} >> "$out"
