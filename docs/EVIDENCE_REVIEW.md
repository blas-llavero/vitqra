# Evidence review and research readiness — v0.4.4 development

This milestone adds local, deterministic documentary review after the existing bounded evidence acquisition. It does not perform peer review, assess medical efficacy, verify eligibility or automatically establish the validity of a scientific challenge.

## Review stages

1. Bind the educational source to the curated identifier and exact problem URL, or bind a Federal Register notice to its identifier and publication date in the source URL. Publication date is not a submission deadline.
2. Check manifest identity, retrieval timestamp, expected file names, byte counts, SHA-256 hashes, source URLs, asset kinds and the existing source-rights warning.
3. Recompute the derived text and repository candidate links from the original stored HTML. Compare with the saved material. An extracted preview can be incomplete.
4. Compare a saved educational sample against the original explicitly labeled sample block. For five introductory exercises, check the expected number of DNA strings, alphabet and applicable length constraints. Other samples are not assessed. These checks do not solve exercises.
5. For a saved official notice PDF, recheck the file signature after the existing exact official URL and hash checks. This is not full PDF validation or digital signature verification.
6. Classify the evidence as unsupported, missing, inconsistent, partial or document checks passed. The localized research row shows a concise status, preserving the directory and controls. An unsuccessful download retains the existing unavailable status.

A report is computed from local evidence, not downloaded or uploaded. The underlying snapshot remains unchanged. The report is in memory and is recomputed when a supported snapshot is acquired or reused. The current research engine is not connected. The report's autonomous research readiness, eligibility, deadline, rights and scientific-validation properties are all false. No future executor may treat a documentary pass as scientific approval. Generic web candidates remain excluded.

## Boundaries

The SHA-256 manifest is not signed and does not prove authenticity against an attacker who changes both the source and manifest. The app-private store and existing HTTPS restrictions are assumed; no stronger local filesystem integrity guarantee is made. Source contents remain untrusted. No additional library, model, dataset, secret, personal contact data or media is distributed. Existing attribution and licensing notices, browser security restrictions and manual/automatic controls are unchanged.

Further work: verify organizer identity, official statement and corrections, eligibility and deadline with source-specific evidence, dataset license and provenance, research methodology, independent reproducibility and expert review. Do not infer these from titles, samples, publication dates or file hashes. Clinical claims and patient data are excluded from this prototype. No automatic application or submission is permitted.

## Testing

Use VITQRA > Run Evidence Review Checks in Unity Editor. Synthetic fixtures cover source identity, publication date mismatch, sample format, missing evidence, coherent snapshot, derived text tampering, hash mismatch and the closed research gate. No Unity execution or device test has been performed. Runtime behavior, storage performance, malformed inputs, translations and Quest 3 rendering still require validation. This development branch is not a release.
