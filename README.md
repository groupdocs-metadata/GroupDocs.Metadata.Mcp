# GroupDocs.Metadata MCP Server

MCP server that exposes [GroupDocs.Metadata](https://products.groupdocs.com/metadata) as AI-callable tools
for Claude, Cursor, GitHub Copilot, and other MCP agents.

## Installation

Requires [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).

**Run directly with `dnx` (recommended — no install step):**

```bash
dnx GroupDocs.Metadata.Mcp@26.4.2 --yes
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

## Available MCP Tools

| Tool | Description |
|---|---|
| `ReadMetadata` | Reads all metadata properties (author, title, creation date, custom properties) and returns them as JSON |
| `RemoveMetadata` | Removes all metadata from a document and saves the cleaned file to storage |

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
      "args": ["GroupDocs.Metadata.Mcp@26.4.2", "--yes"],
      "env": {
        "GROUPDOCS_MCP_STORAGE_PATH": "/path/to/documents"
      }
    }
  }
}
```

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
      "args": ["GroupDocs.Metadata.Mcp@26.4.2", "--yes"],
      "env": {
        "GROUPDOCS_MCP_STORAGE_PATH": "${input:storage_path}"
      }
    }
  }
}
```

## Usage with Docker Compose

```bash
cd docker
docker compose up
```

Edit `docker/docker-compose.yml` to point volumes at your local documents folder.

## License

MIT — see [LICENSE](LICENSE)
