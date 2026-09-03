# Backlog & Known Issues

Running list of ideas, planned work, and known limitations for the
GroupDocs.Metadata MCP server. Grouped by topic. Terse on purpose — each line is
a ticket, not an essay. `[ ]` = open, `[x]` = shipped (kept for context).

**Current surface (26.7.1):** `read_metadata`, `search_metadata`, `write_metadata`,
`remove_metadata` (full + selective `categories`), `get_document_info`.

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

## Tools & functionality

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

- [ ] Add a GPS/EXIF-location JPEG fixture so `search category=gps` and
      `remove categories=[gps]` get positive integration tests (today they're
      mechanism-verified only). **P1**
- [ ] CI leg with a license (`GROUPDOCS_LICENSE` secret) exercising the
      `write_metadata` and selective-remove licensed paths end-to-end. **P1**
- [ ] Confirm 3-OS matrix green after each engine bump (Linux fontconfig, macOS
      libgdiplus).

## Documentation & discoverability

- [ ] `examples-index.json` — machine-readable scenario catalog (intent → tool →
      sample → prompt → expected keys). Only worth it if it doubles as the
      integration-test manifest. **P2**
- [ ] README/AGENTS — dedicated License configuration section (which tools need a
      license, example `.lic` path). **P1**
- [ ] README — "SDK vs Cloud vs MCP" decision snippet. **P2**
- [ ] `llms-full.txt` extended context + `.github/copilot-instructions.md`. **P2**
- [ ] Refresh the MCP Registry description when the tool set changes.
- [x] Cursor how-to + `examples/cursor-mcp.json` (Tests repo, 26.7.1).

## Platform & infra (longer-term)

- [ ] HTTP/SSE transport for shared/team deploys (stdio stays default). **P2**
- [ ] Remote storage (URL / S3) via GroupDocs.Mcp.Core. **P2**
- [ ] `doctor` smoke command (checks .NET, license, storage, tools/list). **P2**

---

*Conventions: any behaviour change ships with a `changelog/NNN-*.md` entry and a
CalVer bump. Integration tests target the published NuGet via `dnx`, so new-tool
tests only pass once the matching version is live.*
