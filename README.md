# GroupDocs.Metadata MCP Server

MCP server that exposes [GroupDocs.Metadata](https://products.groupdocs.com/metadata) as AI-callable tools
for Claude, Cursor, GitHub Copilot, and other MCP agents.

## Installation

```bash
dotnet add package GroupDocs.Metadata.Mcp
```

Or run via Docker:

```bash
docker run --rm -i \
  -v $(pwd)/files:/data/input:ro \
  -v $(pwd)/output:/data/output \
  ghcr.io/groupdocs/metadata-mcp:latest
```

## Available MCP Tools

| Tool | Description |
|---|---|
| `ReadMetadata` | Reads all metadata properties (author, title, creation date, custom properties) and returns them as JSON |
| `RemoveMetadata` | Removes all metadata from a document and saves the cleaned file to storage |

## Configuration

| Variable | Description | Default |
|---|---|---|
| `GROUPDOCS_MCP_STORAGE_PATH` | Base folder for input/output files | current directory |
| `GROUPDOCS_MCP_INPUT_PATH` | Input files folder | `GROUPDOCS_MCP_STORAGE_PATH` |
| `GROUPDOCS_MCP_OUTPUT_PATH` | Output files folder | `GROUPDOCS_MCP_STORAGE_PATH` |
| `GROUPDOCS_LICENSE_PATH` | Path to GroupDocs license file | (evaluation mode) |

## Usage with Claude Desktop

```json
{
  "mcpServers": {
    "groupdocs-metadata": {
      "command": "dotnet",
      "args": ["GroupDocs.Metadata.Mcp.dll"],
      "env": {
        "GROUPDOCS_MCP_STORAGE_PATH": "/path/to/documents"
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
