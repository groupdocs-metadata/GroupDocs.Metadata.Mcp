---
id: 004
date: 2026-04-22
version: 26.4.3
type: fix
---

# MCP Registry NuGet ownership marker + corrected registry URL

## What changed

- [README.md](README.md) — added `<!-- mcp-name: io.github.groupdocs-metadata/groupdocs-metadata-mcp -->` footer marker. The MCP Registry reads this from the published NuGet package as the NuGet-side ownership proof, complementing the GitHub OIDC namespace claim.
- [src/GroupDocs.Metadata.Mcp/.mcp/server.json](src/GroupDocs.Metadata.Mcp/.mcp/server.json) — `packages[0].registryBaseUrl` changed from `https://api.nuget.org` to the fully-qualified V3 index `https://api.nuget.org/v3/index.json`, which is what the Registry's server-side validation expects for `registryType: nuget`.
- [.github/workflows/publish_prod.yml](.github/workflows/publish_prod.yml) — removed the `MCP_REGISTRY_PUBLISH` opt-in variable gate; the `publish_mcp_registry` job now runs on every release via GitHub OIDC, consistent with how NuGet and Docker publishing work.
- [RELEASE.md](RELEASE.md) — dropped the opt-in variable from the secrets/variables table, removed the "One-time MCP Registry setup" section (replaced with a shorter namespace-verification note), and synced `main` → `master` throughout to match the repo's actual default branch.

## Why

26.4.2 reached NuGet and both container registries cleanly but the MCP Registry publish failed twice — first on a URL-format check, then on the missing ownership marker (which NuGet packages treat as immutable, so 26.4.2 couldn't be retro-fixed). 26.4.3 lands all three corrections in one shot so the full four-channel release pipeline (NuGet + ghcr.io + Docker Hub + MCP Registry) publishes end-to-end from a single tag.

## Migration / impact

None for consumers. No API changes, no tool changes. The README gains an HTML-comment footer that's invisible when rendered.
