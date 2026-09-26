#!/usr/bin/env bash
# Builds the consolidated report: concatenates the sections in PLAN.md section 15 order into report.md,
# then converts it to SE3090_G<nn>_Report.pdf with pandoc if it is installed.
#   cd docs/report && ./build.sh            # optional: GROUP=12 ./build.sh
# Fallback without pandoc: open report.md in VS Code (Markdown preview, with a Mermaid extension for the
# diagrams) and use "Print" / "Export to PDF" from the preview, or any Markdown-to-PDF extension.
set -euo pipefail
cd "$(dirname "$0")"

GROUP="${GROUP:-nn}"
OUT_MD="report.md"
OUT_PDF="SE3090_G${GROUP}_Report.pdf"
PAGE_BREAK=$'\n\n<div style="page-break-after: always;"></div>\n\n'

# Diagram order for section 13. A PNG export (docs/diagrams/png/<name>-1.png) replaces the Mermaid source.
diagram() {
  local name="$1" title="$2" png="../diagrams/png/${1}-1.png"
  if [[ -f "$png" ]]; then
    printf '## %s\n\n![%s](%s)\n' "$title" "$title" "$png"
  else
    sed '1s/^# /## /' "../diagrams/${name}.md"
  fi
}

{
  cat 00-cover.md;                      printf '%s' "$PAGE_BREAK"
  for section in 01-overview-scope 02-requirements-roles 03-architecture 04-database \
                 05-api-react-flutter-design 06-technical-report 07-testing-report \
                 08-agent-evaluation-report 09-performance-report 10-deployment-report; do
    cat "${section}.md";                printf '%s' "$PAGE_BREAK"
  done
  cat 11-adrs.md
  for adr in ../adr/ADR-00[1-6]-*.md; do
    printf '\n\n'; sed '1s/^# /## /' "$adr"
  done
  printf '%s' "$PAGE_BREAK"
  cat 12-security.md;                   printf '%s' "$PAGE_BREAK"
  cat 13-diagrams.md; printf '\n\n'
  diagram architecture "System architecture";  printf '\n\n'
  diagram agents "Agent graph";                printf '\n\n'
  diagram workflow "Assessed workflow";        printf '\n\n'
  diagram er "ER diagram";                     printf '%s' "$PAGE_BREAK"
  cat 14-references.md;                 printf '%s' "$PAGE_BREAK"
  cat 15-group-ai-declaration.md;       printf '%s' "$PAGE_BREAK"
  for student in A B C; do
    cat "individual-${student}.md";     printf '%s' "$PAGE_BREAK"
  done
  cat 99-appendix.md
} > "$OUT_MD"

echo "Wrote docs/report/$OUT_MD ($(wc -l < "$OUT_MD" | tr -d ' ') lines, $(grep -c 'TODO' "$OUT_MD") TODO markers left)"

if command -v pandoc >/dev/null 2>&1; then
  pandoc "$OUT_MD" --from gfm --toc --toc-depth=2 --resource-path=.:../diagrams \
    -V geometry:margin=2cm -o "$OUT_PDF"
  echo "Wrote docs/report/$OUT_PDF"
else
  echo "pandoc not found: open docs/report/$OUT_MD in VS Code and print the preview to PDF (see the header of build.sh)."
fi
