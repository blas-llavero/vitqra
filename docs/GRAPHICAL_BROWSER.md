# Graphical browser integration (v0.4.2 update)

The optional OpenWebView adapter displays an interactive Android WebView texture within the existing research window. It uses the MIT-licensed OpenWebView source at 9b7aa413bce9e186fd9277e8bcf193ae48fcf665, which upstream describes as targeting Unity 6, Vulkan, IL2CPP/ARM64 and Quest 3/3S. VITQRA uses the default software capture path initially: HTML, CSS, images and ordinary JavaScript pages are intended to work; hardware video, WebGL, WebXR, PDF rendering, native popup controls and full desktop-browser compatibility are not established. The device WebView provider and XR platform remain platform dependencies, not wholly open-source components controlled by VITQRA.

## Install in a local Unity project

1. Review upstream license, native source, prebuilt native libraries and transitive Android dependencies. Obtain the exact commit in a clean local checkout. No upstream binaries, images or media are included in VITQRA.
2. Run `python tools/install_restricted_webview.py /path/to/upstream-checkout /path/to/UnityProject` from the VITQRA checkout. The script installs a local embedded UPM package and applies exact, revision-checked source transformations. It does not download or publish anything. Do not use the unmodified upstream package with this integration.
3. Review the resulting changes, confirm native binaries correspond to audited sources, and enable `VITQRA_OPENWEBVIEW` in Android Scripting Define Symbols. The adapter references a marker in the patched C# source and is Android-device-only. Without this symbol the original text reader remains the default; no dependency is required in the editor.
4. Build the Unity Android ARM64/IL2CPP application with Internet permission and a current supported Android WebView provider. Test on Quest 3. No APK or dependency has been installed or built in this development environment.

## Browser controls

Select a numbered challenge: the graphical view opens in the same spatial panel when enabled. Back to results preserves the search; Reader opens the original text preview; External browser opens the original challenge URL. The address bar accepts public HTTPS URLs; arrows navigate history, reload refreshes, Type text inserts keyboard text into the focused field, Erase deletes a character, Enter sends the native key. Trigger on webpage content sends down/move/up; the thumbstick scrolls; leaving the content, tracking loss or switching controllers cancels the touch. Grip and resize controls remain independent. Closing destroys the browser and requests clearing browsing data. The browser has no automation hooks into research, notes, credentials, microphone or patient records.

## Restricted upstream changes

Disable initial navigation, JavaScript-to-Unity bridge and related injected page shims, file/content access, mixed content, geolocation, WebView debugging, cookies, automatic popups, debug bitmap export receiver. Reject non-HTTPS and obvious local/nonpublic addresses in native navigation and request interception; cancel SSL, HTTP auth, camera/mic permissions and file chooser. Downloads are not performed. JavaScript still runs within the web page. These are defense-in-depth source changes, not a complete network sandbox: DNS rebinding, redirect behavior in WebView subresources, WebView provider vulnerabilities, page storage, browser logs and platform caching require separate testing. Do not browse private or sensitive sites, sign in, or enter confidential data. No paid service or browser plugin is used.

## Validation

Exact upstream patch anchors were checked against the pinned source. Unity Editor > VITQRA > Run Graphical Browser Checks contains address and UV smoke checks; not executed in Unity. Verify package import, Java and C# compilation, texture orientation, WebView initialization, native permission and SSL denial, navigation restrictions, controller drag/cancel/scroll, keyboard, memory/thermal use, pause/resume, resize and return to directory on hardware. The optional browser is not yet proven usable on Quest. A complete graphical browser with accelerated multimedia requires additional integration and testing.

Upstream: https://github.com/rwpersson/OpenWebView-Unity/tree/9b7aa413bce9e186fd9277e8bcf193ae48fcf665
