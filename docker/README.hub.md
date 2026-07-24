# GroupDocs.Metadata MCP Server

MCP server that exposes [GroupDocs.Metadata](https://products.groupdocs.com/metadata) as AI-callable tools for Claude, Cursor, GitHub Copilot, and other MCP agents.

## Quick start

```bash
docker run --rm -i \
  -v $(pwd)/documents:/data \
  groupdocs/metadata-net-mcp:latest
```

## Use with Claude Desktop

```json
{
  "mcpServers": {
    "groupdocs-metadata": {
      "command": "docker",
      "args": ["run", "--rm", "-i", "-v", "/path/to/documents:/data", "groupdocs/metadata-net-mcp:latest"]
    }
  }
}
```

## Tools

- **ReadMetadata** — Reads all metadata properties (author, title, creation date, custom properties) and returns them as JSON
- **SearchMetadata** — Searches within one document and returns only matching properties (filter by category, name, or value) — e.g. "show me the author", "does this have GPS?"
- **WriteMetadata** — Sets, changes, or adds a property (Author, Title, Subject, Keywords, Comments, Copyright, Company, Manager) and saves the updated file
- **RemoveMetadata** — Removes metadata and saves a cleaned file. Strips everything by default, or only specific `categories` (gps, author, comments, company, dates, software, copyright, keywords, personal)
- **GetDocumentInfo** — Returns the file type, MIME type, page count, byte size, and encryption status as JSON — a lightweight structural check that does not enumerate metadata properties

## Tags & environment

- Tags: `latest` + an immutable version tag per release matching NuGet (e.g. `26.7.1`).
  Platforms: `linux/amd64`, `linux/arm64`. Also on GHCR: `ghcr.io/groupdocs-metadata/metadata-net-mcp`.
- `GROUPDOCS_MCP_STORAGE_PATH` (default `/data`), `GROUPDOCS_MCP_OUTPUT_PATH` (optional),
  `GROUPDOCS_LICENSE_PATH` — mount your license and point at it to leave evaluation mode
  (see the Licensing section in the GitHub README for the exact evaluation limits).

Full docs, one-click installs for other clients, and licensing details:
**https://github.com/groupdocs-metadata/GroupDocs.Metadata.Mcp**
