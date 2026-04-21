---
id: 002
date: 2026-04-21
version: 26.4.1
type: fix
---

# Fix repository URL + bump Core to 26.4.1

## What changed

- `PackageProjectUrl`, `RepositoryUrl`, and `PackageReleaseNotes` in [build/dependencies.props](build/dependencies.props) now use the canonical repo URL `https://github.com/groupdocs-metadata/GroupDocs.Metadata.Mcp`. Previous value (`groupdocs/groupdocs-metadata-mcp`) was wrong on both the org and the casing.
- [src/GroupDocs.Metadata.Mcp/.mcp/server.json](src/GroupDocs.Metadata.Mcp/.mcp/server.json) `repository.url` updated to the same canonical URL and MCP Registry `name` aligned to `io.github.groupdocs-metadata/groupdocs-metadata-mcp` so GitHub OIDC namespace verification succeeds. Both `version` fields bumped to `26.4.1`.
- [README.md](README.md) — `dnx GroupDocs.Metadata.Mcp@…` pinned version updated in all three snippets (install, Claude Desktop, VS Code).
- Upgraded `GroupDocs.Mcp.Core` dependency from `26.4.0` to `26.4.1` (which carries the matching URL fix for the Core packages).

## Why

`git clone` against the hyphenated URL redirected, but deep links like `/releases/tag/26.4.0` on the NuGet "Project website" button did not — users hit a GitHub 404 instead of the release notes.

## Migration / impact

None for consumers upgrading from `26.4.0` to `26.4.1` — no API changes, just metadata corrections. The next nuget.org listing will link correctly.
