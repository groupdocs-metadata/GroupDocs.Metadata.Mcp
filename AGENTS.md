# AGENTS.md — Guide for AI coding agents

Brief orientation for AI coding agents (Claude Code, Copilot, Cursor, Aider, Amp, Codex) working in this repository.

## What this repo is

A standalone **MCP server** for [GroupDocs.Metadata for .NET](https://products.groupdocs.com/metadata) — exposes document metadata operations as AI-callable tools via the Model Context Protocol.

Published to NuGet as `GroupDocs.Metadata.Mcp` with the `McpServer` package type, and to `ghcr.io/groupdocs/metadata-mcp` + `docker.io/groupdocs/metadata-mcp` as a container image.

## MCP tools exposed

| Tool | Description |
|---|---|
| `ReadMetadata` | Extract all metadata (author, title, dates, EXIF, XMP, IPTC, custom properties) from a document and return grouped JSON |
| `RemoveMetadata` | Strip all metadata from a document and write a clean copy to storage |

Both tools accept `FileInput` (resolved via `IFileResolver`) and an optional `password` for protected documents.

## Folder layout

```
src/                                           ← all projects + sln + Directory.Build.props
  GroupDocs.Metadata.Mcp/
    Program.cs                                 ← host bootstrap + stdio transport
    MetadataLicenseManager.cs                  ← applies GroupDocs.Total license
    Tools/
      ReadMetadataTool.cs                      ← [McpServerTool] — ReadMetadata
      RemoveMetadataTool.cs                    ← [McpServerTool] — RemoveMetadata
    .mcp/
      server.json                              ← NuGet.org reads this to generate mcp.json snippet
    GroupDocs.Metadata.Mcp.csproj              ← PackageTypes=McpServer + ToolCommandName
  GroupDocs.Metadata.Mcp.Tests/
  GroupDocs.Metadata.Mcp.sln
  Directory.Build.props
build/
  dependencies.props                           ← single source of truth for all versions
changelog/                                     ← one MD file per change (see changelog/README.md)
docker/
  Dockerfile                                   ← multi-stage, runtime on aspnet:10.0
  docker-compose.yml
.github/workflows/                             ← build_packages.yml, run_tests.yml, publish_prod.yml, publish_docker.yml
```

## Dependencies

- `GroupDocs.Mcp.Core` + `GroupDocs.Mcp.Local.Storage` — infrastructure NuGet packages from the [groupdocs-mcp-core](https://github.com/groupdocs/groupdocs-mcp-core) repo
- `GroupDocs.Metadata` — the actual metadata engine
- `ModelContextProtocol` — MCP SDK for .NET
- `Microsoft.Extensions.Hosting` — host builder for the stdio server

## Commands you can run

```bash
# Restore + build
dotnet restore
dotnet build src/GroupDocs.Metadata.Mcp.sln -c Release

# Run tests
dotnet test src/GroupDocs.Metadata.Mcp.sln -c Release

# Run the server locally (stdio)
dotnet run --project src/GroupDocs.Metadata.Mcp

# Local pack (writes to ./build_out) — validates server.json version matches dependencies.props
pwsh ./build.ps1

# Build + run the Docker image
docker build -f docker/Dockerfile -t metadata-mcp:local .
docker run --rm -i -v $(pwd)/documents:/data metadata-mcp:local
```

## Version scheme

CalVer `YY.MM.N`. The version lives in **two** places that MUST stay in lockstep:
1. `build/dependencies.props` → `<GroupDocsMetadataMcp>`
2. `src/GroupDocs.Metadata.Mcp/.mcp/server.json` → both top-level `"version"` and `packages[0].version`

`build.ps1` enforces this at pack time (`Assert-ServerJsonVersionMatchesDependencies`) — if they drift, the build fails.

## House rules

1. **Tools must have rich `[Description("...")]` strings** — these are what AI agents read via the MCP protocol. Write them as task-oriented sentences, not method-signature summaries.
2. **Never add new env vars beyond** `GROUPDOCS_MCP_STORAGE_PATH`, `GROUPDOCS_MCP_OUTPUT_PATH`, `GROUPDOCS_LICENSE_PATH` without updating `server.json`, `docker-compose.yml`, `README.md`, and [spec 04](../../groupdocs-mcp-framework/specifications/04-github-repo-deployment-of-gd-product.md) together.
3. **Tests use xUnit + Moq** — mock `IFileResolver`, `IFileStorage`, `ILicenseManager`, `OutputHelper`.
4. **Changelog entries required** — any PR that changes behaviour adds `changelog/NNN-slug.md`.
5. **Do not edit `obj/` or `build_out/`** — build artifacts.
6. **Target framework is `net10.0` only** — required by `dnx` and the MCP SDK.

## Release flow

See [RELEASE.md](RELEASE.md) for the exact per-release checklist.

## What NOT to change

- Do not hardcode the version in `.csproj` — it flows from `$(GroupDocsMetadataMcp)` in `dependencies.props`.
- Do not remove the `<PackageTypes>McpServer</PackageTypes>` or `<ToolCommandName>groupdocs-metadata-mcp</ToolCommandName>` from the csproj — NuGet.org discoverability and `dnx` invocation depend on them.
- Do not change the `.mcp/server.json` schema URL without cross-checking with the NuGet MCP docs.
