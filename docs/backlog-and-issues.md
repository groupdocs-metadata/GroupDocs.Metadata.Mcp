# Backlog & Known Issues

Running list of ideas, planned work, and known limitations for the
GroupDocs.Metadata MCP server. Grouped by topic. Terse on purpose — each line is
a ticket, not an essay. `[ ]` = open, `[x]` = shipped (kept for context).

**Current surface (26.9.0):** `read_metadata`, `search_metadata`, `write_metadata`,
`remove_metadata` (full + selective `categories`), `get_document_info`.

---

## Confirmed defects — external audit, 2026-08-16

Source: the Signature + Metadata deep round (202 logged JSON-RPC pairs, Docker vs NuGet channel
parity, library baseline 26.6.0) plus the 12-product sweep, against
`ghcr.io/groupdocs-metadata/metadata-net-mcp:latest` (26.7.3, licensed). 46 family-wide defects
reported and all 46 independently reproduced with control calls. A later validation round found
**zero false positives**.

`S#` = shared core (`GroupDocs.Mcp.Core`) · `M#` = this repo · `P#` = GroupDocs.Metadata library

### MCP wrapper / packaging — this repo

- [ ] **M1** PPTX/PPT completely broken — the Linux image is missing a native library —
      **High**. A whole format family is dark on the Docker channel; the same calls succeed on
      NuGet/Windows, which is why the 39/39 in-repo tests stayed green.
      *Fix:* add the Aspose.Slides native runtime to this image, then add a per-tool smoke test to
      image CI so a whole format family cannot go dark again.
      **P1 — a Dockerfile line plus a CI smoke test.**
- [ ] **M2** `mode:add` reports the wrong reason for failing — **Med**. `affected==0` falls back to
      "not applicable" (`WriteMetadataTool.cs:64-76`), so the caller is sent the wrong way. Also:
      **any mode string other than `add` silently means `set`.**
      *Fix:* distinguish "unsupported mode" from "unsupported property", and reject unknown modes.
      **P1 — one message fix that stops sending callers the wrong way.**
- [ ] **M3** Empty value accepted silently — **Low**. *Fix:* either reject it or name it explicitly
      (`cleared Author`). **P2**
- [ ] **M4** Property categories are weak and undocumented — **Low**. *Impact:* agents filtering by
      category silently miss data. **P2**

### Shared core — fixed once in `GroupDocs.Mcp.Core`, lands here on the next bump

- [ ] **S1** (**M5**) Passing `fileName` crashes any tool — **High**. Unhandled
      `ArgumentException` in `FileResolver.ResolveAsync`; the resolver call sits outside the
      try/catch in every tool (`EraseMetadataTool.cs:34-36` and twins).
- [ ] **S2** (**M6**) Missing files return an opaque error — **High**; listing capped at 20 entries.
- [ ] **S3** `isError` is set on crashes but not on real failures — **Med**.

Nothing to do in this repo for S1–S3 beyond re-testing after the Core bump.

### Product library — upstream

- [ ] **P1** HTML input throws an internal error instead of "not supported" — **Med**.
      *Fix:* either support it or return the standard not-supported message. **P2**

> **Correction recorded by the auditors:** metadata signatures are **not** invisible to every MCP
> tool — this server *does* read them (`xmp:GD.SIGN.QTY`). They are invisible only to the
> Signature server's own tools.

---

## Known issues & limitations

- Evaluation mode blocks `Save()`, so `write_metadata` and `remove_metadata` only
  succeed with a license. By design (paid engine). Read-side tools (`read`,
  `search`, `get_document_info`) work without a license.
- `write_metadata` is predicate-based on the Tags taxonomy: supports the common
  fields (Author, Title, Subject, Keywords, Comments, Copyright, Company, Manager).
  Arbitrary / custom-named properties are not writable yet.
- `search_metadata` `valueContains` matches text only — binary values (e.g. a
  thumbnail) are reported as `<binary N bytes>` and won't match. Intentional.
- Selective `remove_metadata` may leave engine/technical fields (producer,
  creator) behind. It reports what it actually removed; it does not promise a
  guaranteed wipe of those.
- `System.Drawing.EnableUnixSupport` is inert on the current engine (removed in
  System.Drawing.Common 7.0+; engine now ships Aspose.Drawing). Kept as a marker
  until CI confirms `libgdiplus` can be dropped entirely.
- nupkg is ~207 MiB against NuGet's 250 MB ceiling (multi-arch native Skia +
  Aspose.Drawing). Watch this on every engine bump.
- Linux native runs need `libfontconfig1` (Skia dlopens it); macOS needs
  `libgdiplus` via Homebrew. Docker image bundles both.
- **PPTX/PPT are currently dead on the Docker channel** (M1 above) while working on NuGet/Windows.
  The channels are documented as interchangeable and are not.

## Tools & functionality

- [ ] **M2** correct the `mode:add` failure message and reject unknown modes. **P1**
- [ ] **M3** handle empty values explicitly. **P2**
- [ ] **M4** document and strengthen property categories. **P2**
- [ ] `read_metadata` — add `format: summary | full` (default summary; omit
      binary/large values) to keep agent context small. **P1**
- [ ] `write_metadata` — support custom/arbitrary property names and multi-value
      append for list fields (Keywords, Contributors). **P2**
- [ ] `remove_metadata` — write output to the same subfolder as the input, or
      honour `GROUPDOCS_MCP_OUTPUT_PATH` + relative subpath. **P2**
- [ ] `remove_metadata` — `gdpr_basic` preset (PII bundle) once field set is
      agreed with product/legal. **P2**
- [ ] `diff_metadata` — compare metadata properties between two files (not full
      document comparison). **P2**
- [ ] `search_metadata_in_folder` — batch search across storage (separate tool
      from single-file `search_metadata`). **P2**
- [x] `search_metadata`, `write_metadata`, selective `remove_metadata` — 26.7.1.
- [x] `get_document_info` — 26.7.0.
- Rejected for now: `export_metadata_json` (fold into `read_metadata`
  `format=canonical` instead of a near-duplicate tool); standalone
  `list_supported_formats` (low agent value — the engine supports ~110 formats;
  a `check_format_support(ext)` probe would be more useful if anything).

## Testing & CI

- [ ] **Per-tool Linux smoke test in image CI** — one call per tool in the built container. Would
      have caught M1 before release. **P1**
- [ ] Add a `channel: [dnx, docker]` axis — M1 lives in the Linux image and is structurally
      invisible to the current dnx/Windows-only matrix. **P1**
- [ ] PPTX/PPT regression fixture, exercised on the **docker** channel. **P1**
- [ ] Add the two mandatory probes: the **`fileName`-only form**, and a **missing file** asserting
      the promised `Available files:` text. Today's oracle passes on the exact defect. **P1**
- [ ] **Stop counting self-skips as passes** — the audit found missing-fixture cases and
      license-dependent no-ops reported as Passed, overstating coverage. **P1**
- [ ] Add a GPS/EXIF-location JPEG fixture so `search category=gps` and
      `remove categories=[gps]` get positive integration tests (today they're
      mechanism-verified only). **P1**
- [ ] CI leg with a license (`GROUPDOCS_LICENSE` secret) exercising the
      `write_metadata` and selective-remove licensed paths end-to-end. **P1**
- [ ] Confirm 3-OS matrix green after each engine bump (Linux fontconfig, macOS
      libgdiplus).
- [ ] Cold-dnx-cache fragility: first runs failed 100% of tests with init timeouts until the
      CI-style pre-warm was replicated, and left a 0-byte locked nupkg needing process cleanup.
      Keep the prewarm step. **P2**

## Documentation & discoverability

- [ ] Document the PPTX/PPT channel gap until M1 lands. **P1**
- [ ] `examples-index.json` — machine-readable scenario catalog (intent → tool →
      sample → prompt → expected keys). Only worth it if it doubles as the
      integration-test manifest. **P2**
- [ ] README/AGENTS — dedicated License configuration section (which tools need a
      license, example `.lic` path), extended to cover metered. **P1**
- [ ] README — "SDK vs Cloud vs MCP" decision snippet. **P2**
- [ ] `llms-full.txt` extended context + `.github/copilot-instructions.md`. **P2**
- [ ] Refresh the MCP Registry description when the tool set changes.
- [x] Cursor how-to + `examples/cursor-mcp.json` (Tests repo, 26.7.1).

## Platform & infra (longer-term)

- [ ] Metered licensing (`GROUPDOCS_METERED_PUBLIC_KEY` / `_PRIVATE_KEY`) via
      `GroupDocs.Mcp.Core`, plus the `get_license_status` tool. **P1**
- [ ] HTTP/SSE transport for shared/team deploys (stdio stays default). **P2**
- [ ] Remote storage (URL / S3) via GroupDocs.Mcp.Core. **P2**
- [ ] `doctor` smoke command (checks .NET, license, storage, tools/list). **P2**

---

*Evidence: `TEMP_ThirdPartyAnalysis/metadata.md`, `TEST-REPORT.md` (deep round),
`VALIDATION-REPORT.md` (why the green suites miss these). Conventions: any behaviour change ships
with a `changelog/NNN-*.md` entry and a CalVer bump. Integration tests target the published NuGet
via `dnx`, so new-tool tests only pass once the matching version is live.*
