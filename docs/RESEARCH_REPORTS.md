# Research evidence reports — v0.5.2 development

After a supported record completes evidence acquisition and documentary review (including partial review), the queue attempts to save a local report before advancing. Manual mode uses the same path. The existing two-column directory, paging, pause/resume and stop remain unchanged; a short report status is appended to the row. Selecting a challenge still opens its original reader/browser. View report in either detail view loads a saved report into the existing read-only paginated panel, or indicates unavailability. Back returns to the directory. Original website navigation remains available. No new main-list buttons.

## Content and scope

The report records scope, original title, source URL, retrieval timestamp, asset names, sizes and SHA-256 hashes, documentary methodology, results and limitations. The original source preview is referenced by its local file name; a complete challenge statement is not verified or reproduced. A computed result appears only for the five supported educational exercises when an existing checkpoint is rechecked against the original sample and output. The methodology describes the corresponding deterministic algorithm. Other records have no validated sample result. No speculative scientific conclusions, generated citations, model output or clinical advice are introduced. No report is generated for inconsistent/missing/unsupported evidence, including arbitrary web candidates. The scientific readiness gate stays closed.

## Local format and security

UTF-8 plain text and printable HTML are stored together under the app-private Application.persistentDataPath/VITQRA/ResearchReports/<identifier-and-URL SHA-256>/<resolved language>/ directory. The HTML is static, escapes all data, has a restrictive content security policy, and does not embed scripts, remote resources or active hyperlinks. The in-app view uses literal text, not HTML execution. Report headings, scope, limitations and five method descriptions have ten language versions; original titles, source links, hashes and numerical/sequence outputs are unchanged. No translation model is invoked. Arabic HTML uses RTL; headset text layout and fonts require testing. No new external package or API is used.

File size limit 64 KiB each; combined report store ceiling 8 MiB. Files are staged then moved together. Existing report content is compared with reconstruction from verified evidence before reuse or viewing; a mismatch is not overwritten. Report storage is local but unencrypted; source terms apply. No automatic redistribution or submission. The fixed report contains metadata and bounded educational outputs, not copied raw source HTML or contact details. Source-derived title/URL may still contain incidental information. SHA-256 is not digital signature verification or scientific validation. Reports do not include full articles, datasets or patient data. Export/share, deletion, replacement of stale reports, PDF generation and native PDF viewer are pending. A report generated in another language is not automatically created until the record is processed in that language.

## Validation

Run VITQRA > Run Research Report Checks in Unity Editor for synthetic fixtures: HTML escaping, missing evidence, no fabricated result, sample verification, localized output, stable reuse and rejection of changed report files. These tests are supplied but have not been executed in Unity. Compile and test on Quest 3, including actual source retrieval, frame time for synchronous checks, detail controls, pagination, text clipping, RTL/fonts, quotas and interrupted storage. No APK or real scientific report is provided by this source commit. Existing dependency licenses and security restrictions remain unchanged; native dependency and model audits are still pending.

## PDF and ZIP extension

See [RESEARCH_PDF.md](RESEARCH_PDF.md) for the Android-only local PDF and archive source implementation and limitations.
