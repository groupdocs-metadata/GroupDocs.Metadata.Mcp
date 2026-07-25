# Codex CLI (OpenAI)

```bash
codex mcp add groupdocs-metadata -- dnx GroupDocs.Metadata.Mcp --yes
```

Or add to `~/.codex/config.toml`:

```toml
[mcp_servers.groupdocs-metadata]
command = "dnx"
args = ["GroupDocs.Metadata.Mcp", "--yes"]

[mcp_servers.groupdocs-metadata.env]
GROUPDOCS_MCP_STORAGE_PATH = "/path/to/documents"
GROUPDOCS_MCP_OUTPUT_PATH = "/path/to/documents"
GROUPDOCS_LICENSE_PATH = ""   # empty = evaluation mode; set to your GroupDocs.Total.lic to lift limits
```

Pin a version by replacing `GroupDocs.Metadata.Mcp` with `GroupDocs.Metadata.Mcp@26.7.3`.
