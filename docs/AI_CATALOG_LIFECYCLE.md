# Provider catalog lifecycle

All implemented and future AI providers must refresh their available-model inventory on application startup when already configured and authorized, independent of which provider is selected. This requirement does not authorize new accounts, paid requests, new network hosts, credential sharing or model downloads. Catalog refresh is distinct from inference, installation and automatic ranking. Run only one bounded refresh per provider at launch; also refresh after explicit sign-in, account/server change or manual refresh. Do not block the initial UI. Expose loading, available, empty, unauthorized or unavailable states; on failure do not treat stale metadata as current eligibility. Never replace a manually selected unavailable model silently. Apply the research-first selection policy to the current eligible list; update capability rankings separately from availability when verified new information is available. Unknown capabilities remain unranked rather than assumed superior.

## Current implementation

- ChatGPT: official account-specific /v1/models query with the existing authorized session, renewed if needed; implemented in source at workspace launch and after successful explicit sign-in. No catalog inference test or usage reservation. No general ChatGPT research inference enabled. Single account registration only.
- Local llama.cpp: existing local fixed model.gguf provisioning and configuration checks run at workspace setup; only the explicitly enabled, reviewed file is eligible, with SHA-256 verification before inference. There is not yet a multi-model GGUF registry or online catalog. Never scan arbitrary files or auto-download weights.
- Ollama: future adapter, not integrated yet. For an explicitly configured and authorized local or self-hosted instance, use its GET /api/tags endpoint to list installed models. The API returns installed model metadata, not every downloadable model in the public library. Do not discover arbitrary LAN servers, connect to unknown hosts, download models, or start cloud inference. Review transport security and server identity before implementation.
- Other providers: not connected; implement catalog lifecycle for each only after its authorization, transport, privacy, licensing and cost review. If no safe supported listing endpoint exists, report unsupported and use a reviewed manual list instead of scraping or guessing.

All providers must keep the existing no-paid-API-fallback policy. Startup must not transmit challenge content, conversations, patient records, notes or other research content. Do not publish local account metadata or model weights. Preserve existing licenses and attribution.

## Acceptance

Test cold launch without registration (no catalog network call), valid authorized registration with Local AI selected (one catalog request), expired session with allowed refresh, revoked scope, sign-in followed by automatic catalog load, failed network, empty list, duplicate initialization and manual retry. Confirm no inference or counter increment, no unexpected downloads and no change to research gates. Local GGUF verification and future Ollama/other adapters need separate validation. No Android or Unity execution was performed for this source change.

References:
- https://developers.openai.com/siwc/token-sharing-open-source/models-and-inference
- https://docs.ollama.com/api/tags
