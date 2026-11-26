#!/usr/bin/env python3
"""Install the pinned, restricted OpenWebView source package into a local Unity project.
No downloading, publication, dependency installation or network calls are performed here.
"""
import argparse
import json
import pathlib
import shutil
import subprocess
import tempfile

PIN = "9b7aa413bce9e186fd9277e8bcf193ae48fcf665"
PACKAGE = "OpenWebView/Packages/com.reidpersson.openwebview"
PATCHES = json.loads("{\"Plugins/Android/openwebview.androidlib/src/main/java/com/reidpersson/openwebview/OpenWebViewBridge.java\":[[\"    public void loadUrl(final String url) {\",\"    // VITQRA restricted browsing policy. Host checks are syntactic, not a DNS firewall.\\n    private static boolean vitqraHttps(android.net.Uri uri) {\\n        if (uri == null || !\\\"https\\\".equalsIgnoreCase(uri.getScheme()) ||\\n                uri.getUserInfo() != null || (uri.getPort() != -1 && uri.getPort() != 443)) return false;\\n        String h = uri.getHost();\\n        if (h == null) return false;\\n        h = h.toLowerCase(java.util.Locale.ROOT);\\n        if (!h.matches(\\\"[a-z0-9.-]+\\\\\\\\.[a-z][a-z0-9-]*\\\") || h.endsWith(\\\".local\\\") ||\\n                h.endsWith(\\\".internal\\\") || h.endsWith(\\\".localhost\\\") || h.endsWith(\\\".test\\\") ||\\n                h.endsWith(\\\".invalid\\\") || h.endsWith(\\\".example\\\") || h.endsWith(\\\".onion\\\") ||\\n                h.endsWith(\\\".arpa\\\")) return false;\\n        return true;\\n    }\\n\\n    public void pointerCancel(final float nx, final float ny) {\\n        dispatchTouch(MotionEvent.ACTION_CANCEL, nx, ny);\\n        requestFrame();\\n    }\\n\\n    public void loadUrl(final String url) {\\n        if (url == null || !vitqraHttps(android.net.Uri.parse(url))) return;\"],[\"s.setJavaScriptEnabled(true);\",\"s.setJavaScriptEnabled(true);\\n                    s.setAllowFileAccess(false);\\n                    s.setAllowContentAccess(false);\\n                    s.setAllowFileAccessFromFileURLs(false);\\n                    s.setAllowUniversalAccessFromFileURLs(false);\\n                    s.setMixedContentMode(WebSettings.MIXED_CONTENT_NEVER_ALLOW);\\n                    s.setGeolocationEnabled(false);\\n                    android.webkit.WebView.setWebContentsDebuggingEnabled(false);\\n                    android.webkit.CookieManager.getInstance().setAcceptCookie(false);\\n                    android.webkit.CookieManager.getInstance().setAcceptThirdPartyCookies(webView, false);\"],[\"s.setSupportMultipleWindows(true);\",\"s.setSupportMultipleWindows(false);\"],[\"s.setJavaScriptCanOpenWindowsAutomatically(true);\",\"s.setJavaScriptCanOpenWindowsAutomatically(false);\"],[\"holdSslError(handler, error);\",\"handler.cancel();\"],[\"holdHttpAuth(handler, host, realm);\",\"handler.cancel();\"],[\"holdPermission(request);\",\"request.deny();\"],[\"holdGeoPermission(origin, callback);\",\"callback.invoke(origin, false, false);\"],[\"return holdFileChooser(filePathCallback, params);\",\"filePathCallback.onReceiveValue(null); return true;\"],[\"webView.addJavascriptInterface(new JsBridge(), \\\"OpenWebViewNative\\\");\",\"// VITQRA: no page-to-Unity JavaScript interface.\"],[\"registerDumpReceiver();\",\"// VITQRA: debug bitmap export receiver disabled.\"],[\"view.evaluateJavascript(FOCUS_HOOK_JS, null);\",\"// VITQRA: focus bridge disabled.\"],[\"view.evaluateJavascript(SELECT_SHIM_JS, null);\",\"// VITQRA: page shims disabled.\"],[\"view.evaluateJavascript(FORCE_ZOOM_JS, null);\",\"// VITQRA: page shims disabled.\"],[\"view.evaluateJavascript(SELECT_RANGE_JS, null);\",\"// VITQRA: page shims disabled.\"],[\"if (request == null || request.getUrl() == null) return null;\\n                            CustomResource[] res\",\"if (request == null || !vitqraHttps(request.getUrl()))\\n                                return new WebResourceResponse(\\\"text/plain\\\", \\\"utf-8\\\", new java.io.ByteArrayInputStream(new byte[0]));\\n                            CustomResource[] res\"],[\"if (request == null || request.getUrl() == null) return false;\\n                            String scheme\",\"if (request == null || !vitqraHttps(request.getUrl())) return true;\\n                            String scheme\"],[\"void onPageStarted(WebView view, String url, android.graphics.Bitmap favicon) {\",\"void onPageStarted(WebView view, String url, android.graphics.Bitmap favicon) {\\n                            if (url == null || !vitqraHttps(android.net.Uri.parse(url))) { view.stopLoading(); return; }\"]],\"Runtime/OpenWebView.cs\":[[\"[SerializeField] string _initialUrl = \\\"https://www.wikipedia.org\\\";\",\"[SerializeField] string _initialUrl = \\\"\\\";\"],[\"        const string BridgeClass\",\"        public const bool VitqraRestrictedBuild = true;\\n        const string BridgeClass\"],[\"        public void PointerUp(Vector2 n)\",\"        public void CancelPointer(Vector2 n) { _pointerIsDown = false; if (_bridge != null) _bridge.Call(\\\"pointerCancel\\\", n.x, n.y); }\\n        public void PointerUp(Vector2 n)\"]]}")

def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("checkout", type=pathlib.Path, help="Clean upstream git checkout at the pinned commit")
    parser.add_argument("unity_project", type=pathlib.Path)
    args = parser.parse_args()
    checkout = args.checkout.resolve()
    project = args.unity_project.resolve()
    actual = subprocess.check_output(["git", "-C", str(checkout), "rev-parse", "HEAD"], text=True).strip()
    if actual != PIN:
        parser.error("Unexpected upstream revision")
    if subprocess.check_output(["git", "-C", str(checkout), "status", "--porcelain", "--untracked-files=all", "--", PACKAGE], text=True).strip():
        parser.error("Upstream package must be clean")
    source = checkout / PACKAGE
    packages = project / "Packages"
    if not (project / "Assets").is_dir() or not (packages / "manifest.json").is_file():
        parser.error("Expected a Unity project")
    target = packages / "com.reidpersson.openwebview"
    if target.exists():
        parser.error("Existing package must be reviewed and removed manually")
    with tempfile.TemporaryDirectory(dir=packages, prefix=".vitqra-webview-") as tmp:
        staged = pathlib.Path(tmp) / "package"
        shutil.copytree(source, staged)
        for relative, patches in PATCHES.items():
            path = staged / relative
            value = path.read_text(encoding="utf-8")
            for old, new in patches:
                if value.count(old) != 1:
                    raise RuntimeError("Unexpected upstream source: " + relative)
                value = value.replace(old, new, 1)
            path.write_text(value, encoding="utf-8")
        (staged / "VITQRA_RESTRICTED_SOURCE.txt").write_text(PIN + "\n", encoding="utf-8")
        staged.rename(target)
    print("Restricted source package installed locally. Review licenses and native binaries before enabling VITQRA_OPENWEBVIEW.")

if __name__ == "__main__":
    main()
