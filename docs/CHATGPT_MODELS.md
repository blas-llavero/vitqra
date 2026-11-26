# ChatGPT model catalog and usage — v0.5.3c initial development

The Models button inside the single ChatGPT account card opens a secondary view within the same panel. It lists displayable models from the official account catalog in server order; click the model row to cycle manual choices and Automatic. Automatic now uses a research-first preference list of exact documented model IDs, intersected with the current authorized account catalog. It no longer selects the first server entry. The list is reviewed as of 2026-11-26; it prioritizes GPT-6.2 Astra, considers other documented advanced general-purpose models, and follows the published research-task ordering among the four current flagship models. The relative ordering of other models is a provisional engineering preference, not a measured cross-generation benchmark. Unknown or specialized model IDs are not automatically ranked; if no ranked entry is available, the panel asks the user to select a model manually. The manual picker retains server ordering and never substitutes a missing manually selected model. Selection is session-only, and a missing manual slug disables the test rather than silently substituting another model. The local attempt cap defaults to 40 per UTC day, can cycle through 1, 3, 5, 10, 20, 40, and is persisted independently of provider allowances. It counts explicitly started inference tests before dispatch (including failed attempts), not tokens, price, catalog calls or OAuth refresh. Clock rollback blocks new attempts. Actual provider plan limits and credits are separate.

Catalog and test operations run on the existing single Java executor. The token refresh path checks expiry, earliest-refresh time when supplied, uses the issued client ID and replaces the rotating refresh and access tokens together in encrypted storage. Terminal refresh errors clear the session; temporary failures retain it. Only model slug and display name cross JNI, never credentials or claims. Model list and test failures do not trigger automatic retries or paid fallback.

The explicit Test connection button sends only a fixed neutral text prompt to the selected model using the official plan OAuth access token, store=false and stream=true. This consumes plan usage and a local attempt. A response counts as completed only after a completed terminal event; partial streams, failure events and size/time limits are rejected. HTTP 429 or the provider usage-limit event stops the current test, not proof that all plan credits are exhausted. No response text is saved. No tools, attachments, arbitrary prompts, conversation history, research source excerpts or patient data are sent. No automatic inference or external research is enabled. Account and model choices do not enable the existing blocked scientific execution gate. No hidden retries, API key or purchases.

This initial milestone is not an end-to-end ChatGPT research adapter. Persistent model selection across launches, account-specific profiles, explicit research-transfer consent, shared provider interface, quota pause/recovery for the full queue, diagnostic request IDs and complete provider error handling remain required. The existing account connector is still a single-registration prototype. The new test cannot be interrupted mid-request by changing panels; it is bounded by transport timeouts and stream checks, not a strict end-to-end deadline. The account status and test results are separate. A completed event tests transport completion only, not model accuracy or the exact returned text.

No new dependency or media. All UI labels have ten localized variants; verify fit and translations on Quest. Compile Java/C# and test refresh rotation, revoked grants, catalog filtering, model selection, cap persistence, UTC rollover, malformed SSE, incomplete/failed streams, 429, and real account authorization on a headset. No live account call, build or device test has been performed.

Official references:
- https://developers.openai.com/siwc/token-sharing-open-source/models-and-inference
- https://developers.openai.com/siwc/token-sharing-open-source/profiles-and-sessions
- https://developers.openai.com/siwc/token-sharing-open-source/errors-and-recovery
- https://developers.openai.com/siwc/token-sharing-open-source/preview-limitations

## Research model selection checks

Run VITQRA > Run Research Model Selection Checks. Synthetic fixtures exercise order independence, known-model fallback, manual overrides, unavailable models, unknown IDs, misleading names and empty catalogs. The preference list must be reviewed when new models are released; the account catalog does not document a universal quality score. No benchmark, live account call or Unity test was executed in this milestone. No network endpoints, usage limits, credentials, consent or inference gates are changed. The resolved slug is still only used for the explicit connection test until the general research connector is implemented.

Model selection references (reviewed 2026-11-26):
- https://developers.openai.com/api/docs/guides/model-selection
- https://developers.openai.com/api/docs/models
- https://developers.openai.com/api/docs/models/all
