# Scientific evidence acquisition — v0.4.3 development

This source milestone adds bounded retrieval of public source documents using the existing Unity networking and standard C# libraries. No new external software package, credential, API subscription, binary or media is included. Existing dependency licenses and attribution files remain unchanged. The optional graphical browser and its existing security restrictions are not modified. An independent audit of its native binaries and runtime behavior is still required before distribution.

## Behavior

Automatic mode queues eligible records from the filtered numbered list, one at a time, without additional controls. Manual mode queues a supported selected row. Mode or filter changes, app pause and panel disable cancel the current request. Twelve job attempts per app session, two seconds between jobs, 25 seconds per request. Failed attempts are not retried in the same session. Switching to manual mode does not start an automatic job. Retrieval status is localized in all ten interface languages. No scientific resolution is performed.

- Rosalind: original public HTML and derived text preview; extract only an explicitly labeled public Sample Dataset if the expected markup is present. This is an educational example, not a timed competition dataset or research dataset. No login, bypass, or submission.
- Federal Register: public notice HTML and preview. Retrieve a GovInfo PDF only if the source API supplies the exact expected HTTPS document URL matching publication date and document number. Validate MIME and PDF signature; these are not full document validation. A notice is not necessarily a current competition.
- Zenodo/GitHub: record repository link candidates in the local manifest only. Do not fetch, clone or execute arbitrary repositories or data. Source and license verification is pending.
- Generic web candidates are excluded from automatic acquisition. No user search term, private note, conversation, credential or patient record is sent by this adapter. Only eligible source URLs are requested; redirects are disabled and standard TLS validation remains enabled.

## Local evidence

App-private Application.persistentDataPath/VITQRA/ResearchEvidence/<identifier-and-URL SHA-256>/ contains inert source.html.txt, source-preview.txt, optional public-sample.txt or official-notice.pdf, and manifest.json. The manifest records original title, source, UTC retrieval time, file sizes, SHA-256 hashes, source-rights warning and unverified scientific status. Cache entries are checked against the manifest and source identity before reuse. Hashes detect mismatches, not authenticity, licensing or scientific validity. No automatic repair or freshness check. Maximum HTML 2 MiB, PDF 16 MiB, storage 128 MiB with conservative reservations. Staging directories are moved into place after writes; failed writes attempt cleanup. Evidence is not uploaded to GitHub. Public source files may contain incidental personal information; keep them local and do not use restricted or clinical data. No PDF parser, archive extraction or executable content is enabled.

## Review and acceptance

Before publication, scan the full proposed source files for personal email addresses, secrets and confidential data; preserve upstream licenses and copyright notices. The development branch does not constitute a validated release. Unity Editor > VITQRA > Run Evidence Checks provides synthetic parser and URL checks. Run these checks and compile the Android ARM64 project; verify live source responses, rights, interruption, corrupted files, quota and UI on Quest 3. No external dependency binary or live evidence has been validated by this source change. Additional real dataset adapters, document viewing/export/deletion, full statement verification, and local AI resolution remain pending.

References:
- https://www.federalregister.gov/developers/documentation/api/v1
- https://www.govinfo.gov/developers
- https://rosalind.info/faq/
- https://developers.zenodo.org/

## Documentary review

After acquisition or cache reuse, v0.4.4 checks source identity and consistency of the stored evidence. See [EVIDENCE_REVIEW.md](EVIDENCE_REVIEW.md). Scientific validation and autonomous research remain unavailable.
