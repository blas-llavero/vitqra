# Integrated web reader (v0.4.2)

The research directory opens a read-only text preview in its existing movable and resizable spatial window when the user selects a numbered challenge. The directory and its current search remain in memory, and background discovery can continue. Back to results restores the directory. The original title and URL are shown only in the detail view. The controller thumbstick or previous/next page buttons paginate the text. An explicit External browser button opens the validated original URL in the platform browser.

## Scope and security

The reader is implemented entirely in C# using Unity's existing networking and UI. It is not a full graphical browser, DOM parser or verified scientific statement extractor. It does not run scripts, display images, execute HTML, load subresources, support forms or follow hyperlinks. It accepts only reviewed source routes on rosalind.info and www.federalregister.gov, HTTPS without credentials or query strings. No redirects, 15-second timeout, 2 MiB transfer limit; HTML MIME types only. It performs bounded, heuristic text extraction, removes common non-content elements, decodes entities, and displays literal text. It assumes UTF-8 and can omit or distort navigation, tables, layout, equations and text from dynamic pages. It is limited to 48,000 UTF-16 units, split into pages of approximately 480 grapheme clusters. Original titles and text remain untranslated. Results are not interpreted as instructions or used as scientific evidence automatically.

Other search candidates show their title and original URL but are not fetched in-app. The user may explicitly open them in the external browser; syntactic URL checks do not establish trust or eligibility. No automatic navigation to arbitrary sites is enabled. Closing the reader cancels its active request and discards text. No files or browsing history are saved by the reader. Underlying platform networking may have its own cache/cookie behavior; do not enter sensitive information. No API fees or new binary dependencies.

## Future graphical browser

Open-source options to audit include TLabWebView (Android WebView/GeckoView texture integration) and OpenWebView-Unity (Quest world-space WebView). Verify pinned license, transitive binaries, Android ARM64/IL2CPP/Vulkan support, security updates, HTTPS and navigation restrictions, input, lifecycle and resource limits before integrating. Neither plugin is installed or redistributed in this milestone. No paid browser package is approved.

## Tests

Unity Editor > VITQRA > Run Web Reader Checks covers URL allowlisting, text extraction and pagination with synthetic fixtures. No editor execution or physical test has been performed. Verify HTTPS response/redirect failures, content types, Unicode and text clipping, controller scroll, close during download, scale, and return to directory on Quest. Test both real sources; a site may block requests or require scripts. No APK or graphical browser is provided.

References:
- https://github.com/TLabAltoh/TLabWebView
- https://github.com/rwpersson/OpenWebView-Unity

## Graphical update

An optional graphical OpenWebView adapter is now implemented in source. See [GRAPHICAL_BROWSER.md](GRAPHICAL_BROWSER.md). This read-only reader remains the fallback when the graphical package is not installed or enabled.
