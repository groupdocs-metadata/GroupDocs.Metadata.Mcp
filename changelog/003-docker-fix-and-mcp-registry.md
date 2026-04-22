---
id: 003
date: 2026-04-22
version: 26.4.2
type: fix
---

# Fix Docker image + enable MCP Registry publishing

## What changed

- [docker/Dockerfile](docker/Dockerfile) — switched from a custom `adduser` step to `USER $APP_UID`, the non-root user pre-shipped by `mcr.microsoft.com/dotnet/aspnet:10.0` (UID 1654). The previous `adduser` command was Debian-specific and broke on the Azure Linux base image that Microsoft now uses for .NET 10.
- [src/GroupDocs.Metadata.Mcp/.mcp/server.json](src/GroupDocs.Metadata.Mcp/.mcp/server.json) — shortened `description` to fit the MCP schema's 100-character cap and dropped inaccurate "write" wording (the server only exposes `ReadMetadata` and `RemoveMetadata`, no write tool).
- [.github/workflows/run_tests.yml](.github/workflows/run_tests.yml) — replaced the ajv-cli schema-validation step with `check-jsonschema`, which handles the MCP schema's cross-draft `$ref` to the draft-07 meta-schema natively instead of tripping ajv's strict mode.
- MCP Registry publishing enabled — `MCP_REGISTRY_PUBLISH=true` repo variable now set, so `publish_prod.yml → publish_mcp_registry` runs on every release via GitHub OIDC.

## Why

The 26.4.1 release succeeded on NuGet but the Docker image build blew up on the `adduser` step (Azure Linux base doesn't ship Debian's `adduser` wrapper), and the MCP Registry step was gated behind an opt-in variable that hadn't been set — so 26.4.1 was never listed at `registry.modelcontextprotocol.io`. 26.4.2 is the end-to-end verification that all three channels (NuGet + Docker + MCP Registry) publish cleanly from a single release.

## Migration / impact

None for consumers. No API changes, no behaviour changes — same two tools, same env vars, same schema.
