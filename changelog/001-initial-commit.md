---
id: 001
date: 2026-04-18
version: 26.4.0
type: feature
---

# Initial public release of GroupDocs.Metadata MCP Server

## What changed
- NuGet package `GroupDocs.Metadata.Mcp` published with `McpServer` package type.
- Two MCP tools exposed:
  - `ReadMetadata` — extracts all metadata properties (author, title, creation date, custom properties, EXIF, XMP, IPTC) and returns grouped JSON.
  - `RemoveMetadata` — strips all metadata from a document and writes a clean copy to storage.
- Installable via `dnx GroupDocs.Metadata.Mcp@26.4.0 --yes` (.NET 10 SDK required) or `dotnet tool install -g`.
- Docker image published to `ghcr.io/groupdocs/metadata-mcp` and `docker.io/groupdocs/metadata-mcp`.
- Environment variables: `GROUPDOCS_MCP_STORAGE_PATH`, optional `GROUPDOCS_MCP_OUTPUT_PATH`, `GROUPDOCS_LICENSE_PATH`.

## Why
First product MCP server in the GroupDocs MCP framework. Exposes GroupDocs.Metadata
for .NET as an AI-callable tool for Claude, Cursor, VS Code / GitHub Copilot, and
other MCP-compatible agents.

## Migration / impact
First release — no migration required.
