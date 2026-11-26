# Automatic challenge queue and educational sample verification — v0.5.1 development

This milestone extends the existing bounded evidence queue with persistent Pause/Resume and Stop controls beneath the directory. The two-column area layout, search, manual/automatic selector and previous/next page controls are unchanged. Paging never changes job selection. Automatic mode processes eligible filtered items in directory order; manual mode processes a selected supported item. The existing 12-attempt per-session ceiling and delays remain. Pausing or stopping aborts the active network request and prevents subsequent evidence jobs. Resume allows subsequent eligible work. Completed in-memory states remain; canceled active work can be retried within the remaining attempt budget. Manual selection while paused queues at most one item, not an unbounded list. Stop clears the current pending manual request. App pause and panel disable still cancel the request. The persisted control preference stores only running/paused/stopped, not search terms or personal data. It is distinct from the existing app lifecycle pause. It does not freeze discovery or the browser. The current filter and full job list are not persisted. On restart cached evidence is rechecked before reuse.

## First executable scope

Five curated introductory Rosalind public educational samples can be solved by fixed, bounded C# algorithms after the existing document and sample consistency checks pass: nucleotide counts, DNA transcription, reverse complement, Hamming distance and overlapping motif positions. The original source HTML must contain exactly one recognized Sample Output block. A matching result is labeled Sample output matches, not a solved challenge or validated scientific finding. Missing/ambiguous markup, unsupported inputs, mismatches and storage failures do not count as success. Input sequences are bounded to 1000 characters each. The sample is not a real timed contest dataset. No login, submission, downloaded code execution, native model invocation or external tool is allowed in this path. Source terms apply; no data or output is redistributed.

The app stores at most one small sample-check.json per supported curated challenge under app-private Application.persistentDataPath/VITQRA/EducationalResults/<identifier-and-URL SHA-256>/. It includes the computed result, source and input hashes, expected and actual output hashes, original URL, timestamp and explicit educational-only scope. On reuse it recomputes the answer and compares the checkpoint against current evidence. Existing inconsistent files are not silently overwritten. A checksum is not proof of authenticity. No research result is fabricated. The other curated exercises, government notices and web candidates cannot be treated as solved by this implementation.

## Scientific and AI gate

VitqraEvidenceReview.Report.CanStartAutonomousResearch remains false. General research requires verified challenge statements, eligibility, rights, appropriate data, audited inference, execution isolation and scientific validation. The v0.5.0 model bridge remains conversational only and is not called by the queue. No patient-specific or clinical research workflow is enabled. Local AI-generated scientific answers cannot replace reproducible execution or expert assessment.

## Acceptance

Run VITQRA > Run Educational Sample Checks for synthetic algorithm and parser tests. No Unity compilation or physical test has been executed. Verify actual source markup and terms, cache consistency, stop/resume and restart behavior, cancellation, result status, storage, ten-language UI and Quest 3 performance. Local synchronous sample checking is bounded but may still cause frame delay when reading and hashing evidence. Broader job persistence, approved data adapters, scientific method validation, sandboxed code execution and report generation remain pending. No new external library, model, binary, credentials or multimedia are distributed.

## Local reports

See [RESEARCH_REPORTS.md](RESEARCH_REPORTS.md) for v0.5.2 report generation after documentary review and sample checks.
