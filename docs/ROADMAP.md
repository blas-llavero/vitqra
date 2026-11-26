# Roadmap

**Current gate:** retain Unity/OpenXR as selected by the author; continue v0.3.3 with local speech integration, opt-in persistent chat and window resizing. The local-only settings guard, opt-in single-conversation persistence and optional microphone/transcription bridge are implemented in source. Sherpa-ONNX STT/TTS source adapters are added in v0.3.4; native compilation, model provisioning, device validation, and multiple conversations remain pending. Cross-session window layout persistence is added in v0.3.6; editor and device verification remain pending. Independent resizing source is added in v0.3.5; editor and headset tests remain pending. No Godot migration is planned. See [OPEN_SOURCE_POLICY.md](OPEN_SOURCE_POLICY.md).

1. Source overlay, desktop preview, README, settings and notes (this package).
2. Unity XR rig and passthrough via the Mixed Reality template; repositionable world-space panels and controller input source added in v0.2.0. Editor import, template configuration and physical Quest 3 tests remain pending.
3. v0.3.6: persistent user-relative panel layout and conversation visibility; v0.3.5: independent proportional resizing and reset controls with half-second edge hover activation and automatic hiding; v0.3.4 localization update: device-controlled localization and English repository baseline; v0.3.4: local neural speech source adapters and optional playback; v0.3.3: optional local chat persistence and speech lifecycle bridge; v0.3.2: larger conversation panel, in-session history, text composer, keyboard and future voice hooks; v0.3.1: on-demand avatar settings; v0.3.0: four selectable procedural avatar profiles, localized advice and optional XR Hands OK gesture source. Unity/device tests, international font/RTL validation, natural local speech, dictation and reviewed avatar models remain pending.
4. v0.4.2 graphical update: optional restricted OpenWebView adapter, address/navigation and controller input; native installation and hardware validation pending. v0.4.2: integrated read-only source page reader with pagination and return to results; graphical browser, source verification and device tests pending. v0.4.1: optional self-hosted SearXNG web candidate discovery, automatic area/keyword queries and localized status; source verification and deployment pending. v0.4.0: BIOQRA-style numbered directory, ten area filters, keyword search, persisted resolution mode, direct curated Rosalind exercises and initial Federal Register notice adapter; live API/device tests, additional biomedical and hackathon adapters, embedded browser and document import remain pending.
5. v0.5.0 development: optional in-process CPU llama.cpp conversational adapter and local model integrity gate; native build, model audit, headset tests, autonomous research, job checkpointing and report generation pending. No commercial providers. v0.5.1 development: persistent queue pause/stop state and five deterministic educational sample checks; scientific research gate remains closed. v0.5.2 development: local structured documentary reports and educational sample results in static HTML/text, ten-language report templates and integrated text viewing; v0.5.3 development: Android-native generated PDF, verified local report ZIP and in-panel PDF rendering; device tests, full data archives, export and general scientific research pending.
6. Human-centered competitive research: v0.5.2 research extension adds two-pass local exploratory drafts and human-impact critique; full statement retrieval, verified data, benchmark execution, iteration and scientific validation remain pending. See [HUMAN_CENTERED_RESEARCH.md](HUMAN_CENTERED_RESEARCH.md). Separate consent-based patient and affected-person community only after privacy, security and clinical governance review.
7. Multiplayer avatar presence and optional video.
8. Author-reviewed logo, five store visuals, English About and Help videos (max 30 seconds each), then nine localized versions of each, narration and subtitles. No publication before approval.
9. Quest build, physical device testing, accessibility and store requirements review.

Configuration extension: two provider choices in the initial XR settings panel; local remains default and the ChatGPT preference is pending official account connection. Model listing, authorized account usage, translation execution and headset validation remain pending. See [AI_PROVIDER_SETTINGS.md](AI_PROVIDER_SETTINGS.md).

v0.5.3b development: optional Android account sign-in, identity and scope checks, encrypted session record and sign-out controls. Single registration only; live tests, refresh, multi-account support and inference remain pending. See [CHATGPT_ACCOUNT.md](CHATGPT_ACCOUNT.md).

v0.5.3c initial: official model catalog, on-demand token refresh, local daily attempts and explicit neutral connection test. General inference and automatic research pending live tests and consent integration.

Catalog lifecycle requirement: refresh configured and authorized providers automatically at launch; ChatGPT startup and post-sign-in refresh implemented in source. Ollama and other provider adapters must implement this lifecycle when integrated. See [AI_CATALOG_LIFECYCLE.md](AI_CATALOG_LIFECYCLE.md).

Startup selection requirement: prefer current official research rankings intersected with each configured provider's eligible catalog. ChatGPT public-guidance parser and reviewed fallback implemented in source; live-format/device validation and other providers pending.

v0.5.3c.1 development: bounded two-pass ChatGPT educational drafts in the automatic queue, explicit consent, common usage cap and provider-specific local reports. General external research, live validation and conversation integration pending.

v0.5.3d development: Ollama private HTTPS catalog refresh, reviewed digest priority, provider settings and bounded educational drafts; live tests, dynamic capability ranking and general research pending. See [OLLAMA.md](OLLAMA.md).

v0.5.4 initial: measured naive/KMP motif comparison, deterministic synthetic suite and verified local benchmark report extension. General iterative AI optimization and independent benchmarks remain pending. See [ALGORITHM_OPTIMIZATION.md](ALGORITHM_OPTIMIZATION.md).

v0.5.4a development: AI-guided choice and one feedback revision for the reviewed motif algorithms across all three providers, separate synthetic evaluation and report comparison. General iterative optimization pending. See [AI_MOTIF_REVISION.md](AI_MOTIF_REVISION.md).

v0.5.5 initial: structured eight-dimension human-impact research questions, unknown-state handling and report/package integration. Independent impact evidence, expert review and clinical/community release remain pending. See [HUMAN_IMPACT.md](HUMAN_IMPACT.md).

## Author-requested longitudinal and preventive healthcare extension

After v0.5.6, add synthetic longitudinal trajectories (v0.5.7), controlled retrospective research (v0.5.8) and experimental multimodal evaluation (v0.5.9). Expand the existing v0.6 community roadmap into separate clinical, research and peer-support workspaces with clinician, patient, researcher and industry roles. Clinical use requires independent release gates; no real patient intake or individual prediction is currently enabled. See [LONGITUDINAL_HEALTH_RESEARCH.md](LONGITUDINAL_HEALTH_RESEARCH.md).

v0.5.6: local reproducibility record, sample and synthetic benchmark rechecks, draft provenance and explicit blocked clinical/scientific release states in viewer and report package. Independent validation and Unity/device tests pending. See [REPRODUCIBILITY.md](REPRODUCIBILITY.md).

v0.5.6a: unsigned independent-review request bound to report and reproducibility hashes; formal review, signatures, institutional and regulatory approval workflow required in future phases. No certificate or clinical release. See [SCIENTIFIC_CLINICAL_VALIDATION.md](SCIENTIFIC_CLINICAL_VALIDATION.md).

v0.5.7–v0.5.8: synthetic timeline and retrospective experiment integrated into development at fb9171a; Unity tests pending. See [SYNTHETIC_LONGITUDINAL_AND_RETROSPECTIVE.md](SYNTHETIC_LONGITUDINAL_AND_RETROSPECTIVE.md).

v0.5.9 initial: closed synthetic multimodal proxy comparison, paired complete-case evaluation, coverage and conditional Wilson intervals, editor-only preview. Disease-specific data access, feature extraction, clinical validation and runtime XR remain pending. See [SYNTHETIC_MULTIMODAL.md](SYNTHETIC_MULTIMODAL.md).

v0.6.0 initial: closed fictional role/resource/purpose/action authorization policy, expiry and revocation fixtures, editor test console and boundary/sample-day checks. No live authentication, server, real clinical data or collaboration. See [COMMUNITY_ACCESS_POLICY.md](COMMUNITY_ACCESS_POLICY.md).

v0.6.1 initial: fictional previsit summary with current authorization and separate historical cutoff, missingness, provenance, descriptive trends and editor preview. No actual intake, export, clinical prediction or runtime XR. See [SYNTHETIC_PREVISIT.md](SYNTHETIC_PREVISIT.md).

v0.6.2 initial: fixed fictional aggregate collaboration, exact purpose and resource permissions, group/cell/complement suppression, shared release digest and localized editor preview. No real disclosure, patient education delivery, clinical claims or live collaboration. See [SYNTHETIC_AGGREGATE_COLLABORATION.md](SYNTHETIC_AGGREGATE_COLLABORATION.md).

v0.6.2a initial: author-requested data completeness and informative sampling quality gate, strict synthetic scheduled coverage, separate event entries, cutoff and missingness audit. Ready for review is not research authorization. Future authorized dataset integration pending. See [DATA_QUALITY_AND_BIAS.md](DATA_QUALITY_AND_BIAS.md).

v0.6.2b initial: interactive fictional six-state daily diary, scheduled/extra distinction, voluntary simulated reminder and device authorization, separate coverage counts and preserved prevalence. No actual intake, device integration or OS notifications. See [SYNTHETIC_DAILY_DIARY.md](SYNTHETIC_DAILY_DIARY.md).
