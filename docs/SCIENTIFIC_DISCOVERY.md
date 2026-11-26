# Scientific challenge discovery (v0.4.0 — directory redesign)

The research panel now uses a BIOQRA-style numbered directory: a search field at the top, a single manual/automatic resolution preference, ten scientific areas plus All topics on the left, and a numbered list on the right. It has no refresh, individual next-result, source, portal or separate search buttons. Select an area or finish entering search text to filter immediately. The right-hand list displays six results per page with continuous numbering, total count above the list, and Previous (left), page indicator (center) and Next (right) beneath the results only. Buttons remain visible but dimmed and have no effect at the corresponding boundaries. The thumbstick also changes result pages. New area/keyword selections reset to page one; incoming results preserve the current page where possible. Empty results display 0 / 0. The left area column remains unchanged. Paging does not change the research queue; select a numbered row to open its optional graphical browser (or integrated text fallback), with an explicit external browser action. The panel is 0.86 × 0.90 m by default; older stored research positions are skipped once to avoid overlapping My Notes. Other layout preferences are preserved.

## Data and coverage

- Twelve direct Rosalind educational problem links are reused from BIOQRA's reviewed snapshot (data/challenges.json, commit cc87138d652c843bf909d47e65755cd1d38d3bf6; reviewed date 2026-11-14). They are labeled educational, not live research competitions. Generic benchmark index links are excluded.
- The existing Federal Register public API fetches three fixed terms at startup, up to 40 records per query. Incoming results appear progressively. Its records are explicitly labeled unverified notices; they are not established open competitions or ready-to-run scientific tasks. Dates and eligibility require official verification.
- The ten areas use English source keywords with localized area labels. Typing a matching localized area name selects that area. Arbitrary keywords filter loaded titles and tags locally. If an explicitly authorized, self-hosted SearXNG endpoint is configured, a public research term also triggers an optional general web candidate search. See [WEB_DISCOVERY.md](WEB_DISCOVERY.md). No matching result means an empty list, not an invented challenge. A source failure is distinct from an empty filter.
- Source names and URLs are not shown in the main list; validated original links and IDs remain internal. Row selection opens the graphical browser when enabled, otherwise the integrated read-only reader. Graphical package installation, device validation and translation are pending. See [WEB_READER.md](WEB_READER.md).
- Additional direct challenge sources (including live Grand Challenge, Sage/DREAM, NASA and general STEM sources), verified web challenge statements, verified deadlines, challenge statements and open datasets are future integration work. No automatic fetching of arbitrary result websites; only the configured search service and implemented public endpoint are queried.

## Manual and automatic mode

Both modes share the same persisted automaticResearch preference, automatic by default. Selection takes effect immediately and the AI Settings panel reflects it; no extra Save button. The research engine is not connected yet, so neither mode claims to solve challenges. The automatic runner must eventually consume only verified challenge records, retrieve the statement and datasets, generate research artifacts, pause/resume/cancel and continue to the next challenge after successful completion or an explicit recorded failure. Unverified notices and educational exercises require separate handling. No commercial AI API, application submission or fabricated solution is allowed.

## Validation

Compile in Unity and verify controller scrolling, keyboard, six numbered visible rows, area switching, empty results, startup refresh, mode persistence, legacy layout migration and original link selection on Quest. Existing parser checks remain applicable. New directory checks exercise local filtering, direct links, deduplication and classification; not executed in Unity. No native APK or real scientific resolution is provided.

References:
- https://github.com/blas-llavero/bioqra/blob/cc87138d652c843bf909d47e65755cd1d38d3bf6/data/challenges.json
- https://rosalind.info/problems/list-view/
- https://www.federalregister.gov/developers/documentation/api/v1

## Result pagination checks

Use VITQRA > Run Result Page Checks in Unity Editor. The included checks cover empty results, page boundaries, incomplete last pages and changes in result count. They have not been executed in Unity. Verify layout, localization and controller clicks on Quest 3 before release. No research security controls or external dependencies are changed.
