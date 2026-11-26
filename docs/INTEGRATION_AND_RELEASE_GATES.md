# Integration and release gates — v0.9.0 initial

Source preparation only, not a compiled release, complete security audit or clinical validation. Baseline e2bc7104a8f91de99e9aa9a8e5288f86fb5a6672: 185 tracked files, 104 C# files, two Java files, one C and one C++ file. All 34 existing editor check files are explicitly listed, with one new synthetic evidence-chain suite. Total 35 suites, not individual assertions. Selective source review, no Unity execution.

## New integration fixture

The new test builds an invented educational sample in a unique OS temporary directory and cleans it in finally. It invokes the existing evidence review, sample solver, report, package content, reproducibility and unsigned review request APIs. It checks all ten locales, Arabic HTML direction, absence of an AI draft, stable identity, clinical release blocked, edited report, changed checkpoint and changed source. No download, real data, AI or signature. It tests package content, not Android PDF rendering or ZIP generation. The synthetic clinical modules remain isolated; no real patient-to-research pipeline is enabled.

## Run the offline reference checks

From the repository root, run `python3 verification/run_offline_checks.py` with Python 3, Node.js and Python's `cryptography` package (version 42 or newer, required by the digital trust check). Once the dependencies are installed, the command runs offline, compares each standalone synthetic script's JSON output with its checked-in `results.json`, and returns a nonzero exit code on failure. These scripts reproduce synthetic reference results independently of the C# implementation. Unity import, the 35 editor suites, Android and Quest checks require the corresponding environment below.

## Run in a disposable local test project

1. Create a Unity 6 LTS Mixed Reality template project following QUEST_SETUP.md. Record exact Unity version, resolved Packages/manifest.json and packages-lock.json. This repository is a source overlay, not a complete Unity project; no generated scene, ProjectSettings or resolved package lock is present.
2. Copy UnityStarter/Assets/VITQRA into Assets. Configure the two Java files Android-only as documented in CHATGPT_ACCOUNT.md and RESEARCH_PDF.md. Finish import and fix compiler errors. No actual model, native optional binary or account is needed for offline tests.
3. Save a truly empty scene with no objects; exit Play mode. Select VITQRA > Run Integration Checks (Empty Test Scene). Run offline, without confidential data or real account credentials. The runner calls the 35 explicit suites, reports pass/fail/not-run, and stops if objects remain. It never changes scenes, saves settings or builds. Some underlying fixtures create temporary files/objects or generate ephemeral signing material. No automatic startup. A logged error or caught exception marks failure. Inspect local console. Do not share raw logs without redaction and review.
4. Optional batch method Vitqra.VitqraIntegrationRunner.RunBatch uses the same preconditions and exits 0 for pass, 1 for failure, 2 for unmet preconditions. Compilation failure may prevent the method from running. Interactive execution preferred until scene availability is verified. Record actual suite results, not expected ones.
5. Then test desktop preview and proper MR scene separately. Verify existing layout, clear center, hidden avatars, movement, resizing, restoration, ten locales, glyphs and RTL. Test Android ARM64/IL2CPP and physical Quest 3, Java imports, permissions, lifecycle, memory, accessibility and offline fallback. No APK publication until approved and verified.

## Pending release gates

Unity import/compile, all editor suites, Android compilation and device acceptance remain untested. Optional llama.cpp source pin, sherpa-onnx source/library match and restricted OpenWebView provenance require complete dependency/license/security review and actual native builds. Preserve upstream attribution; Unity and necessary platform components remain author-approved proprietary exceptions. No model or media publication without approval. No paid API or silent fallback. Review provider/account security, storage, backup exclusion, revocation, actual egress, browser restrictions and logs. Static inventory is not a security audit. All ten languages require full translation, glyph and accessibility review, not merely locale-key checks.

Clinical, community, telepresence and object recognition are still demonstrations. Actual clinical intake, authorized datasets, independent evidence, real signing trust, network calls between people and camera/tracking require their own technical and governance gates. No real clinical release. Do not merge main or mark v1.0.0 based on source availability. Next step: real Unity import and compilation, fix observed errors and rerun suites. No change to runtime workspace behavior.
