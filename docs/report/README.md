# Report sources

One Markdown file per section of the consolidated report (PLAN.md section 15 order), pre-filled from the
repository. `TODO` marks every screenshot, URL, measured number or hand-written text still to add.

| Order | File |
|-------|------|
| Cover (members, links, accounts) | [00-cover.md](00-cover.md) |
| 1 | [01-overview-scope.md](01-overview-scope.md) |
| 2 | [02-requirements-roles.md](02-requirements-roles.md) |
| 3 | [03-architecture.md](03-architecture.md) |
| 4 | [04-database.md](04-database.md) |
| 5 | [05-api-react-flutter-design.md](05-api-react-flutter-design.md) |
| 6 | [06-technical-report.md](06-technical-report.md) |
| 7 | [07-testing-report.md](07-testing-report.md) |
| 8 | [08-agent-evaluation-report.md](08-agent-evaluation-report.md) |
| 9 | [09-performance-report.md](09-performance-report.md) |
| 10 | [10-deployment-report.md](10-deployment-report.md) |
| 11 | [11-adrs.md](11-adrs.md) + the six files in [../adr/](../adr/README.md) |
| 12 | [12-security.md](12-security.md) |
| 13 | [13-diagrams.md](13-diagrams.md) + [../diagrams/](../diagrams/README.md) |
| 14 | [14-references.md](14-references.md) |
| 15 | [15-group-ai-declaration.md](15-group-ai-declaration.md) |
| Individual reports | [individual-A.md](individual-A.md), [individual-B.md](individual-B.md), [individual-C.md](individual-C.md) |
| Appendix | [99-appendix.md](99-appendix.md) |
| Addendum (after submission; not in the built report) | [addendum-v1.1.md](addendum-v1.1.md): what changed in v1.1 and why, with the v1.1 test results. v1.0 is the submitted state. |

**Build:** `./build.sh` (optionally `GROUP=<nn> ./build.sh`) writes `report.md` and, if pandoc is installed,
`SE3090_G<nn>_Report.pdf`. Without pandoc: open `report.md` in VS Code and print the preview to PDF. Both outputs
are git-ignored. Export the diagrams to PNG first so they appear as images (`../diagrams/README.md`).
