# GroupDocs.Metadata MCP Server

MCP server that exposes [GroupDocs.Metadata](https://products.groupdocs.com/metadata) as AI-callable tools
for Claude, Cursor, GitHub Copilot, and other MCP agents.

## Installation

Requires [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).

**Run directly with `dnx` (recommended — no install step):**

```bash
dnx GroupDocs.Metadata.Mcp --yes
```

Pulls the latest stable release on every invocation. To pin to a specific
version (recommended for shared configs and CI), append `@<version>`:

```bash
dnx GroupDocs.Metadata.Mcp@26.7.1 --yes
```

**Or install as a global dotnet tool:**

```bash
dotnet tool install -g GroupDocs.Metadata.Mcp
groupdocs-metadata-mcp
```

**Or run via Docker:**

```bash
docker run --rm -i \
  -v $(pwd)/documents:/data \
  ghcr.io/groupdocs-metadata/metadata-net-mcp:latest
```

## Native prerequisites

The underlying GroupDocs engine uses `System.Drawing` (GDI+) for some
operations. When you run the server **natively** (via `dnx` or the global
dotnet tool) on Linux or macOS, install the native `libgdiplus` library first:

| Platform | Setup |
|---|---|
| Windows | Nothing — GDI+ is built into the OS. |
| Linux | `sudo apt-get install -y libgdiplus libfontconfig1` |
| macOS | `brew install mono-libgdiplus` |
| Docker | Nothing — the image already bundles `libgdiplus`. |

Skipping this on Linux/macOS surfaces as `DllNotFoundException: libgdiplus` in
the tool response. The simplest zero-setup option on Linux/macOS is the
**Docker image**.

## Available MCP Tools

| Tool | Description |
|---|---|
| `ReadMetadata` | Reads all metadata properties (author, title, creation date, custom properties) and returns them as JSON |
| `SearchMetadata` | Searches within one document and returns only matching properties (filter by category, name, or value) — e.g. "show me the author", "does this have GPS?" |
| `WriteMetadata` | Sets, changes, or adds a property (Author, Title, Subject, Keywords, Comments, Copyright, Company, Manager) and saves the updated file |
| `RemoveMetadata` | Removes metadata and saves a cleaned file. Strips everything by default, or only specific `categories` (gps, author, comments, company, dates, software, copyright, keywords, personal) |
| `GetDocumentInfo` | Returns the file type, MIME type, page count, byte size, and encryption status as JSON — a lightweight structural check that does not enumerate metadata properties |

## Configuration

| Variable | Description | Default |
|---|---|---|
| `GROUPDOCS_MCP_STORAGE_PATH` | Base folder for input and output files | current directory |
| `GROUPDOCS_MCP_OUTPUT_PATH` | *(Optional)* separate folder for output files | `GROUPDOCS_MCP_STORAGE_PATH` |
| `GROUPDOCS_LICENSE_PATH` | Path to GroupDocs license file | (evaluation mode) |

## Usage with Claude Desktop

```json
{
  "mcpServers": {
    "groupdocs-metadata": {
      "type": "stdio",
      "command": "dnx",
      "args": ["GroupDocs.Metadata.Mcp", "--yes"],
      "env": {
        "GROUPDOCS_MCP_STORAGE_PATH": "/path/to/documents"
      }
    }
  }
}
```

> To pin to a specific version, replace `"GroupDocs.Metadata.Mcp"` with
> `"GroupDocs.Metadata.Mcp@26.7.1"` in `args`. Pinning is recommended for
> shared / committed configs to avoid surprise upgrades.

## Usage with VS Code / GitHub Copilot

NuGet.org generates a ready-to-use `mcp.json` snippet on the [package page](https://www.nuget.org/packages/GroupDocs.Metadata.Mcp).
Copy it directly into your `.vscode/mcp.json`.

Alternatively, add manually to `.vscode/mcp.json`:

```json
{
  "inputs": [
    {
      "type": "promptString",
      "id": "storage_path",
      "description": "Base folder for input and output files.",
      "password": false
    }
  ],
  "servers": {
    "groupdocs-metadata": {
      "type": "stdio",
      "command": "dnx",
      "args": ["GroupDocs.Metadata.Mcp", "--yes"],
      "env": {
        "GROUPDOCS_MCP_STORAGE_PATH": "${input:storage_path}"
      }
    }
  }
}
```

> Same pinning rule as above — swap `"GroupDocs.Metadata.Mcp"` for
> `"GroupDocs.Metadata.Mcp@26.7.1"` to lock to a specific release.

## Usage with Docker Compose

```bash
cd docker
docker compose up
```

Edit `docker/docker-compose.yml` to point volumes at your local documents folder.

## Documentation & guides

Step-by-step deployment guides and a published-package integration test suite
live in the companion repo
[**GroupDocs.Metadata.Mcp.Tests**](https://github.com/groupdocs-metadata/GroupDocs.Metadata.Mcp.Tests):

- [Install from NuGet](https://github.com/groupdocs-metadata/GroupDocs.Metadata.Mcp.Tests/blob/master/how-to/01-install-from-nuget.md) — `dnx`, global tool, pinned vs always-latest
- [Run via Docker](https://github.com/groupdocs-metadata/GroupDocs.Metadata.Mcp.Tests/blob/master/how-to/02-run-via-docker.md)
- [Verify on the MCP registry](https://github.com/groupdocs-metadata/GroupDocs.Metadata.Mcp.Tests/blob/master/how-to/03-verify-mcp-registry.md)
- [Use with Claude Desktop](https://github.com/groupdocs-metadata/GroupDocs.Metadata.Mcp.Tests/blob/master/how-to/04-use-with-claude-desktop.md)
- [Use with VS Code / GitHub Copilot](https://github.com/groupdocs-metadata/GroupDocs.Metadata.Mcp.Tests/blob/master/how-to/05-use-with-vscode-copilot.md)
- [Use with Cursor](https://github.com/groupdocs-metadata/GroupDocs.Metadata.Mcp.Tests/blob/master/how-to/07-use-with-cursor.md)
- [Run the integration tests](https://github.com/groupdocs-metadata/GroupDocs.Metadata.Mcp.Tests/blob/master/how-to/06-run-integration-tests.md)

That repo also exercises every advertised tool against the **published** NuGet
artifact on Linux, macOS, and Windows in CI — so the snippets above are
verified end-to-end on every release.

## License

MIT — see [LICENSE](LICENSE)

<!-- mcp-name: io.github.groupdocs-metadata/groupdocs-metadata-mcp -->
