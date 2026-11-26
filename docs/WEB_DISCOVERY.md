# Universal web discovery (v0.4.1)

VITQRA can optionally query a user-controlled SearXNG instance using its documented JSON search interface. The client uses no paid API, credentials or default public instance. SearXNG is an open-source metasearch engine (AGPL-3.0); it runs separately from the Unity application. Hosting resources and internet access are not necessarily cost-free. Upstream search engines may limit requests or require separate terms. Configure only engines that do not require payment and whose terms permit the intended usage.

## One-time configuration for this source prototype

1. Install a reviewed SearXNG release on a computer or server you control, following upstream documentation. Enable JSON in search.formats. Do not expose an unauthenticated instance to the public internet.
2. Make its /search endpoint reachable by the headset over HTTPS with a valid trusted certificate. HTTP is accepted only for loopback editor development; do not bypass TLS checks or enable insecure Android networking.
3. In Unity, select the spatial workspace and set Self Hosted Search Endpoint to the exact /search URL. Explicitly enable Allow Public Search Terms only after understanding that typed public research terms are sent to the instance and its configured upstream engines. No notes, conversations, patient records or credentials are sent by this adapter. Never enter confidential data in the search field.
4. Build and test. No additional buttons appear in the research window. Selecting an area or finishing a keyword entry triggers the optional web search automatically. All topics retains the startup catalog and public notices; it does not make an unbounded web search. A general web search is not enabled until configured. There is no fallback to a public instance.

Two query variants (challenge and competition) are issued sequentially after a short debounce. A newer selection invalidates old responses; at most one web search runs at a time. Each request is limited to 15 seconds, 2 MiB and 30 accepted candidates, with redirects disabled. Original titles and links are retained; heuristic filtering rejects obvious index pages and non-HTTPS links. Results are labeled unverified web candidates, not confirmed official challenge statements. The app never automatically visits arbitrary result URLs; a deliberate row selection opens the integrated reader detail view; a separate explicit action opens the external browser. Arbitrary web candidates are not fetched by the integrated reader. Hostname checks are syntactic only, not proof of public DNS or trust. No automatic scientific resolution or submissions may consume these candidates until a separate statement, source, deadline and eligibility verification pipeline is implemented.

The existing curated Rosalind exercises and Federal Register notices remain available. Search failures and unconfigured state are localized. UI and content translation limits remain unchanged. Runtime endpoint configuration UI, autonomous source-page extraction, source verification, global coverage and local AI research remain pending. No server or APK has been deployed. The editor checks are provided but not executed in Unity.

References:
- https://docs.searxng.org/dev/search_api.html
- https://docs.searxng.org/admin/installation.html
- https://github.com/searxng/searxng
