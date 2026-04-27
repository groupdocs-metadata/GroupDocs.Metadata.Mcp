---
id: 005
date: 2026-04-27
version: 26.4.4
type: fix
---

# Linux runtime fixes (SkiaSharp + libgdiplus), README polish, and Docker image OCI labels

## What changed

### Linux native asset fix (the real production bug)

- [build/dependencies.props](build/dependencies.props) — added `<SkiaSharp>3.119.0</SkiaSharp>` under "External Dependency Versions" with an inline comment requiring this property to stay in lockstep with whatever SkiaSharp version `$(GroupDocsMetadata)` transitively pulls (ABI-incompatible otherwise).
- [src/GroupDocs.Metadata.Mcp/GroupDocs.Metadata.Mcp.csproj](src/GroupDocs.Metadata.Mcp/GroupDocs.Metadata.Mcp.csproj) — added `<PackageReference Include="SkiaSharp.NativeAssets.Linux.NoDependencies" Version="$(SkiaSharp)" />`. Inline comment flags this as a workaround for the upstream `GroupDocs.Metadata` nuspec gap and notes it can be dropped once the upstream nuspec declares the platform native asset packages.

**Before:** the published 26.4.3 nupkg shipped `tools/net10.0/any/runtimes/{win-*,osx}/native/libSkiaSharp.{dll,dylib}` but **no `linux-*/native/libSkiaSharp.so`** — `GroupDocs.Metadata`'s nuspec only declares `<dependency id="SkiaSharp" version="3.119.0" />` (managed assembly only), so `dnx`-installed packages on Ubuntu had nothing to load when SkiaSharp's `SKObject..cctor` ran. The integration tests crashed with `SkiaSharpVersion.CheckNativeLibraryCompatible` → unhandled finalizer exception → `MCP server process exited unexpectedly` on every Ubuntu CI job.

**After:** the 26.4.4 nupkg ships all 12 Linux native variants (`linux-x64`, `linux-arm64`, `linux-musl-x64`, `linux-arm`, `linux-x86`, `linux-loongarch64`, `linux-riscv64`, plus the four `linux-musl-*` siblings). Verified via `Expand-Archive` of the locally-built nupkg; the previously-empty `runtimes/linux-*` paths now contain the expected `libSkiaSharp.so` files (8.8 MB for `linux-x64`).

### libgdiplus / System.Drawing.Common Linux fix (the second Linux production bug)

- [docker/Dockerfile](docker/Dockerfile) — added an `apt-get install -y --no-install-recommends libgdiplus libfontconfig1` step in the `final` stage (under `USER root`, with cache cleanup after) so the runtime image has the GDI+ implementation that `System.Drawing.Common` 6.0.0 P/Invokes on Linux.
- [src/GroupDocs.Metadata.Mcp/GroupDocs.Metadata.Mcp.csproj](src/GroupDocs.Metadata.Mcp/GroupDocs.Metadata.Mcp.csproj) — added `<RuntimeHostConfigurationOption Include="System.Drawing.EnableUnixSupport" Value="true" />`. Emitted into the published `runtimeconfig.json`; without it, `System.Drawing.Common` 6.0.0's static initializer throws `PlatformNotSupportedException` on first bitmap allocation regardless of whether `libgdiplus` is present.

**Before:** reading metadata from a JPEG (`sample.jpg` from the integration tests) through the published Docker image produced this stack:
```
System.TypeInitializationException: The type initializer for 'Gdip' threw an exception.
 ---> System.PlatformNotSupportedException: System.Drawing.Common is not supported on non-Windows platforms.
   at System.Drawing.LibraryResolver.EnsureRegistered()
   at System.Drawing.SafeNativeMethods.Gdip.PlatformInitialize()
   at System.Drawing.Bitmap..ctor(Int32 width, Int32 height)   ← inside GroupDocs.Metadata
```

PDF/DOCX/XLSX paths were unaffected (those don't go through `Bitmap`). Only image formats (JPEG, PNG, image-bearing PDFs) tripped this.

**After:** the runtime config flag flips the gate, and `libgdiplus.so` is loadable from the container's `/usr/lib/x86_64-linux-gnu/`. JPEG/PNG `read_metadata` calls succeed.

### README polish — install snippets default to unpinned

- [README.md](README.md) — three install snippets reworked to lead with the unpinned form and show pinning as the explicit alternative:
  1. **Top install snippet** — `dnx GroupDocs.Metadata.Mcp --yes` is now the lead, with `@26.4.4` shown below as the "for shared configs / CI" pinning option.
  2. **Claude Desktop config block** — `"args": ["GroupDocs.Metadata.Mcp", "--yes"]` with a callout explaining how to swap in the pinned form.
  3. **VS Code / Copilot config block** — same treatment.
- [README.md](README.md) — new **"Documentation & guides"** section above the License heading, linking to the companion [GroupDocs.Metadata.Mcp.Tests](https://github.com/groupdocs-metadata/GroupDocs.Metadata.Mcp.Tests) repo and its six per-channel how-to guides (NuGet install, Docker, MCP registry verification, Claude Desktop, VS Code / Copilot, integration tests). Includes a one-sentence note that the Tests repo exercises every advertised tool against the **published** NuGet artifact on Linux / macOS / Windows in CI.

### Docker image OCI labels

- [docker/Dockerfile](docker/Dockerfile) — added `ARG VERSION=dev` and a `LABEL` block on the `final` stage with seven OCI standard labels: `image.title`, `image.description`, `image.version`, `image.source`, `image.url`, `image.licenses`, `image.vendor`.
- [.github/workflows/publish_docker.yml](.github/workflows/publish_docker.yml) — added `build-args: VERSION=${{ steps.version.outputs.version }}` to the `docker/build-push-action@v6` step, so each release substitutes the real package version into the label at build time.

## Why

**Linux native asset fix:** the `GroupDocs.Metadata` published nuspec declares only `<dependency id="SkiaSharp">` — neither `SkiaSharp.NativeAssets.Linux.NoDependencies` nor any other native-asset package is listed. This is fine for normal application consumers (the .NET runtime loads natives from the consumer's resolved package graph), but `<PackAsTool>true</PackAsTool>` packages a self-contained tool layout — the `runtimes/<rid>/native/` content is only copied for RIDs whose native-asset packages are in *this* project's resolved graph. Win32 and macOS native libs reach the MCP package transitively through some other Aspose dep; Linux doesn't. Adding the explicit `SkiaSharp.NativeAssets.Linux.NoDependencies` reference here patches the gap until the upstream `GroupDocs.Metadata` nuspec is fixed to declare it directly. Tied to `$(SkiaSharp)` so the lock-step is one property edit at every upstream bump rather than scattered.

**libgdiplus / System.Drawing.Common Linux fix:** Microsoft made `System.Drawing.Common` Windows-only by default in .NET 6 — non-Windows callers throw `PlatformNotSupportedException` from the static initializer unless the `System.Drawing.EnableUnixSupport` runtime config flag is set. The flag must be declared on the **entry-point project** (the one that emits `runtimeconfig.json`), not on a referenced library — so even though the upstream `GroupDocs.Metadata.csproj` source has it, the flag doesn't propagate to consumers. Combined with the missing `libgdiplus` system package in the Debian-based runtime image (the `mcr.microsoft.com/dotnet/aspnet:10.0` base intentionally omits it for image-size reasons), every JPEG/PNG `read_metadata` call on Linux failed. Both pieces are required: opt-in alone gives `DllNotFoundException`, libgdiplus alone gives `PlatformNotSupportedException`. The fix has a hard expiry — the `EnableUnixSupport` flag was removed in `System.Drawing.Common` 7.0+; `GroupDocs.Metadata` 26.x pins 6.0.0 so the flag still works. If upstream ever bumps past 6.0.0 without migrating off `System.Drawing`, this stops working and the consumer needs a different escape hatch.

**README polish:** users upgrading the package on their own cadence (every 1–2 months in the typical case) shouldn't have to copy-paste a new version into their MCP client config every release. The unpinned form is the natural default for solo / dev use; pinning is the deliberate choice for committed / CI configs. Mirrors how `npx` / `dnx` are typically used elsewhere. The new Documentation & guides section makes the companion test repo (which already provides per-channel walkthroughs) discoverable from the canonical README — previously users hit the package page with no hint that step-by-step guides exist outside this repo.

**Docker image OCI labels:** the published image inherited `org.opencontainers.image.version` from the base `mcr.microsoft.com/dotnet/aspnet:10.0` image, which sets the label to the **Ubuntu OS version** (`24.04`). Verified by inspecting `ghcr.io/groupdocs-metadata/metadata-net-mcp:latest`:

```
$ docker inspect ... --format '{{index .Config.Labels "org.opencontainers.image.version"}}'
24.04
```

Effect: every release pushed to ghcr.io / Docker Hub since the project began carries the same `24.04` version label, regardless of whether the underlying GroupDocs.Metadata.Mcp version is 26.4.0 or 26.4.3. The label is one of the standard ways tooling (Renovate, Dependabot, registry UIs, security scanners) infers what's running, and a wrong/stuck value defeats them.

After this fix, `docker inspect ghcr.io/groupdocs-metadata/metadata-net-mcp:26.4.4` will return `"version":"26.4.4"`. Local `docker build` runs without `--build-arg VERSION=...` get `version=dev` — self-identifying as a non-release build instead of inheriting the misleading `24.04`.

The other six labels (title, description, source, url, licenses, vendor) are static — they don't need to change between releases — but were missing entirely from the inherited set, so registries and clients couldn't surface basic metadata about what the image is or who publishes it.

## Migration / impact

- **No API changes, no tool changes, no breaking config changes.** The csproj edit produces a larger published nupkg (~8 MB extra for the Linux native libs), the Docker image grows ~3 MB compressed for libgdiplus + libfontconfig1, and consumers see the same MCP tool surface. The Docker label additions are pure metadata — container behaviour is identical.
- **For users on Linux:** 26.4.3 was effectively broken in two stages — `dnx` crashed during finalization on first SkiaSharp use (server never started), and even when it ran (Docker), JPEG/PNG metadata reads threw `PlatformNotSupportedException`. 26.4.4 is the first release the `dnx` flow works on Linux at all, **and** the first release where Docker image consumers can read image-format metadata on Linux. macOS / Windows users are unaffected by both fixes — those native libs were already shipping correctly.
- **CI hosts on bare Ubuntu** (e.g. integration tests using the dnx flow on `ubuntu-latest` GitHub runners) need `apt-get install -y libgdiplus libfontconfig1` as a workflow step too — the csproj `EnableUnixSupport` opt-in handles the runtime-side gate, but the system package is still required and `dnx` doesn't ship one. The Docker flow handles both pieces inside the image.
- **Downstream consumers** that read OCI labels (security scanners, image-management UIs, Renovate version detection) will start seeing correct values from 26.4.4 onward. Earlier releases on the registries (26.4.0 → 26.4.3) keep their `24.04` label since image manifests are immutable.
- **Local Docker builds:** `docker build -f docker/Dockerfile -t metadata-net-mcp:test .` now produces an image whose `version` label is `dev`. To pin during local validation, pass `--build-arg VERSION=26.4.4`.
- **For maintainers:** when bumping `<GroupDocsMetadata>` in `dependencies.props`, re-verify `<SkiaSharp>` matches the new transitive version. Run `dotnet pack` and confirm `runtimes/linux-x64/native/libSkiaSharp.so` is still present in `tools/net10.0/any/`. ABI mismatch produces the same `CheckNativeLibraryCompatible` crash from the consumer side. Worth adding to [RELEASE.md](RELEASE.md) step 1 as a sub-bullet next to the existing version bumps.
- **Optional follow-up for [RELEASE.md § 6 Post-release verification](RELEASE.md):** add a step *"Pull `:NEW_VERSION` from ghcr.io and assert `docker inspect ... --format '{{index .Config.Labels "org.opencontainers.image.version"}}'` returns NEW_VERSION."* — catches regressions if the workflow's `build-args` line ever drifts.
