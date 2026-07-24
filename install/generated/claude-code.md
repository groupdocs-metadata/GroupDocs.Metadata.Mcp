# Claude Code

```bash
claude mcp add groupdocs-metadata -- dnx GroupDocs.Metadata.Mcp --yes
```

With storage folder and license:

```bash
claude mcp add groupdocs-metadata -e GROUPDOCS_MCP_STORAGE_PATH=/path/to/documents -e GROUPDOCS_LICENSE_PATH=/path/to/GroupDocs.Total.lic -- dnx GroupDocs.Metadata.Mcp --yes
```

Pin a version by replacing `GroupDocs.Metadata.Mcp` with `GroupDocs.Metadata.Mcp@26.7.2`.
