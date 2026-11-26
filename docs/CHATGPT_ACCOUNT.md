# ChatGPT account connection — v0.5.3b development

Optional development-only account sign-in from the initial XR AI settings panel. Local AI remains the default. Selecting ChatGPT account does not sign in automatically; the user presses Sign in and authorizes access in the system browser. Manage usage opens the official ChatGPT usage page. The panel displays connection state only, never an email address, access token or refresh token. The effective local-only inference gate and blocked research queue for the selected external provider remain unchanged. **No ChatGPT inference, model list, automatic research submission or API billing is enabled in this milestone.**

The original Java source uses the documented open-source OAuth registration flow, independent VITQRA host identifier, app name and issued client ID. It creates a loopback-only callback listener with fresh random state, nonce and PKCE S256 verifier, bounded HTTP parsing and timeout. The token exchange uses the issued client identifier, HTTPS with standard certificate validation, fixed issuer origin and no redirects. The ID token is checked using the published RSA JWKS, issuer, audience, nonce, subject and timestamps. Plan grant status requires both requested plan and resource scopes actually returned and a bearer access token. It is not an entitlement check or evidence of a successful inference request. Browser authorization alone does not prove success. No raw network errors, tokens or identity claims are returned to Unity or logged by this code.

The encrypted private record uses Android Keystore AES-GCM with a VITQRA-specific key. It retains a single account registration, its verified subject, encrypted token set and granted scopes; profile/email claims are not copied into separate fields. The identity token itself may contain such claims as required by the documented flow. Nothing is stored in Unity PlayerPrefs or exported. Local sign-out clears credentials and retains registration and subject, then attempts refresh-token revocation using the issuer's discovery document and same-origin HTTPS endpoint. A failed remote revocation is reported; the user can revoke the app in ChatGPT settings. Cancellation closes the listener and blocks late completion for a superseded attempt. Remote permission already granted may still need manual revocation. Switching to Local AI cancels an in-progress browser sign-in and never silently falls back to ChatGPT. Browser pause is expected during authorization.

This is a single-registration development prototype. Multiple account registrations, safe account selection, automatic token refresh, reauthorization recovery after expiry, verified model catalog, quota admission, request limits, separate explicit consent for external research transfer, and completed-response handling are required before any inference is enabled. The current user can reconnect the same verified subject; switching identities is not supported in this milestone. A selected ChatGPT plan may consume plan allowances or existing credits; no API key or automatic purchase path is supplied. ChatGPT is a proprietary optional service and not open-source inference.

## Build and acceptance

Import the Java source as an Android-only Unity plugin, API 23 or newer, preserving its public static methods when code shrinking. Existing Internet permission is needed; no cleartext network permission expansion or WebView authentication is introduced. Browser callback delivery on Quest, Android activity lifecycle, app backup exclusion, key invalidation, cancellation and account revocation require physical testing. The JNI adapter is platform-gated and returns Unsupported in the editor. This code has not been compiled or tested against a live account, and is not production-ready. A separate security review of OAuth, local storage and account lifecycle is required. No dependency, credential, external media or model is added.

References:
- https://developers.openai.com/siwc/token-sharing-open-source/sign-in
- https://developers.openai.com/siwc/token-sharing-open-source/profiles-and-sessions
- https://developers.openai.com/siwc/token-sharing-open-source/errors-and-recovery
- https://developers.openai.com/cookbook/articles/sign-in-with-chatgpt

## Model and usage development extension

See [CHATGPT_MODELS.md](CHATGPT_MODELS.md). The catalog, on-demand session refresh and explicit fixed-text connection test are implemented in source. General inference and automatic research remain disabled.
