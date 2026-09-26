# Diagrams

All diagrams are Mermaid, rendered by GitHub and by VS Code's Markdown preview (with a Mermaid extension).

| Diagram | File |
|---------|------|
| ER diagram (from the EF Core model) | [er.md](er.md) |
| System architecture | [architecture.md](architecture.md) |
| Assessed workflow, sequence with approval pause | [workflow.md](workflow.md) |
| Agent graph with the re-plan loop | [agents.md](agents.md) |

PNG exports go in `png/`. They are not generated yet (mermaid-cli is not installed); to create them:

```bash
cd docs/diagrams && mkdir -p png
for f in er architecture workflow agents; do npx -y @mermaid-js/mermaid-cli -i $f.md -o png/$f.png -b white; done
```

(mermaid-cli renders every ```mermaid block in the file, numbering the outputs `png/<name>-1.png`.)
