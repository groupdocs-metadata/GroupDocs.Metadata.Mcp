# Release Process — GroupDocs.Metadata.Mcp

End-to-end checklist for releasing a new version to NuGet.org + ghcr.io + Docker Hub.

## Versioning — CalVer `YY.MM.N`

- `YY` — 2-digit year (e.g. `26` = 2026)
- `MM` — month without leading zero (e.g. `4` = April)
- `N` — patch increment starting at `0`; increment for hotfixes within the same month

Example: `26.4.0`, `26.4.1`, `26.5.0`.

## Per-release checklist

### 1. Prepare the changelog entry

Add a new file under [changelog/](changelog/):

```
changelog/NNN-short-slug.md
```

Use the next sequential `NNN` and follow the frontmatter in [changelog/README.md](changelog/README.md).
Set `version: {NEW_VERSION}` in the frontmatter.

### 2. Bump the package version in `build/dependencies.props`

```xml
<GroupDocsMetadataMcp>{NEW_VERSION}</GroupDocsMetadataMcp>
```

### 3. Bump `.mcp/server.json` (TWO places)

Edit [src/GroupDocs.Metadata.Mcp/.mcp/server.json](src/GroupDocs.Metadata.Mcp/.mcp/server.json) and update **both** version fields:

```json
{
  "version": "{NEW_VERSION}",          ← top-level
  "packages": [
    {
      "version": "{NEW_VERSION}",      ← inside packages[0]
      ...
    }
  ]
}
```

> `build.ps1` validates that both `server.json` version fields match `<GroupDocsMetadataMcp>` in `dependencies.props`. If they drift, the build fails — this is the safety net.

### 4. (Rarely) bump dependency versions

Only when needed — update the `External Dependency Versions` block in
[build/dependencies.props](build/dependencies.props):

- `GroupDocsMcpCore` — bump if new infra features are needed
- `GroupDocsMetadata` — bump to track new GroupDocs.Metadata engine releases
- `MicrosoftExtensionsHosting` — bump with .NET LTS patches
- `ModelContextProtocol` — bump with MCP SDK releases
- `MicrosoftSourceLinkGithub` — rarely

### 5. (Rarely) bump tool versions

- `CodeSignTool` version — GitHub repository variable `CODE_SIGN_TOOL_VERSION`. Bump under **Settings → Secrets and variables → Actions → Variables**.

### 6. Update the pinned version in `README.md`

Search [README.md](README.md) for the old version number and replace every occurrence (`dnx GroupDocs.Metadata.Mcp@...`, Claude Desktop config, VS Code `mcp.json`). Four references typically.

### 7. Verify locally

```powershell
# This runs the server.json ↔ dependencies.props consistency check
./build.ps1

# Tests
dotnet test src/GroupDocs.Metadata.Mcp.sln -c Release

# Docker build sanity check
docker build -f docker/Dockerfile -t metadata-mcp:test .
```

### 8. Commit

```bash
git add build/dependencies.props src/GroupDocs.Metadata.Mcp/.mcp/server.json README.md changelog/NNN-*.md
git commit -m "Release {NEW_VERSION}"
git push
```

### 9. Wait for CI green

`build_packages.yml` and `run_tests.yml` must pass on `main`.

### 10. Tag the release

```bash
git tag {NEW_VERSION}
git push origin {NEW_VERSION}
```

**No `v` prefix.** Tag must match `[0-9]+\.[0-9]+\.[0-9]+`.

### 11. CI takes over

Two workflows fire in parallel on the tag push:

**`publish_prod.yml`** (NuGet):
1. Builds with `BUILD_TYPE=PROD`
2. Runs `build.ps1` → validates server.json + dependencies.props, packs .nupkg + .snupkg
3. Signs with SSL.com eSigner
4. Pushes to NuGet.org using `NUGET_API_KEY_PROD`
5. Creates GitHub Release with the changelog entry attached

**`publish_docker.yml`** (container images):
1. Builds multi-arch image (linux/amd64, linux/arm64)
2. Pushes to `ghcr.io/groupdocs/metadata-mcp:{NEW_VERSION}` + `:latest`
3. Pushes to `docker.io/groupdocs/metadata-mcp:{NEW_VERSION}` + `:latest`

### 12. Post-release verification

- [ ] NuGet: package listed at new version, signed badge visible, "MCP Server" tab shows generated `mcp.json`
- [ ] GitHub Release created with artifacts + changelog body
- [ ] `ghcr.io/groupdocs/metadata-mcp:{NEW_VERSION}` pullable
- [ ] `docker.io/groupdocs/metadata-mcp:{NEW_VERSION}` pullable
- [ ] Smoke test `dnx GroupDocs.Metadata.Mcp@{NEW_VERSION} --yes` from a clean machine

## Required GitHub secrets & variables

**Secrets** (`Settings → Secrets and variables → Actions → Secrets`):

| Secret | Purpose |
|---|---|
| `NUGET_API_KEY_PROD` | NuGet.org API key scoped to `GroupDocs.Metadata.Mcp` |
| `ES_USERNAME` | SSL.com eSigner username |
| `ES_PASSWORD` | SSL.com eSigner password |
| `ES_TOTP_SECRET` | SSL.com eSigner TOTP 2FA secret |
| `CODE_SIGN_CLIENT_ID` | SSL.com eSigner OAuth CLIENT_ID |
| `DOCKERHUB_USERNAME` | Docker Hub username for `groupdocs` org |
| `DOCKERHUB_TOKEN` | Docker Hub access token scoped to `groupdocs/metadata-mcp` |

**Variables** (`Settings → Secrets and variables → Actions → Variables`):

| Variable | Default | Purpose |
|---|---|---|
| `CODE_SIGN_TOOL_VERSION` | `1.3.0` | CodeSignTool release tag from github.com/SSLcom/CodeSignTool |
| `MCP_REGISTRY_PUBLISH` | *(unset)* | Set to `true` to enable the MCP Registry publish step. Requires one-time namespace setup with the MCP Registry (`io.github.groupdocs/*`). |

> `GITHUB_TOKEN` is provisioned automatically — no setup needed for ghcr.io pushes or MCP Registry OIDC auth.

## One-time MCP Registry setup

Before enabling `MCP_REGISTRY_PUBLISH=true`:

1. Verify namespace ownership with the MCP Registry — either:
   - **GitHub OIDC** (recommended): publish from a repo inside `github.com/groupdocs/*` — the Registry auto-verifies the `io.github.groupdocs/*` namespace via OIDC claims.
   - **DNS**: add a TXT record to `groupdocs.io` per the Registry's DNS verification docs.
2. Test the first publish with a release candidate tag; confirm the server appears at `https://registry.modelcontextprotocol.io/servers?name=io.github.groupdocs/groupdocs-metadata-mcp`.
3. Flip `MCP_REGISTRY_PUBLISH` to `true` for subsequent auto-publishing on every tag.

## Yanking a bad release

1. NuGet.org: unlist (don't delete) the bad package
2. ghcr.io / Docker Hub: delete the affected tag (but keep `:latest` pointing at the previous good release)
3. Bump `N` in `dependencies.props` + `.mcp/server.json`
4. Add a `type: fix` changelog entry
5. Tag and release the patch
