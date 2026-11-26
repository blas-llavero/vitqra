# Local artificial intelligence — v0.5.0 development

The optional in-process llama.cpp adapter connects the existing conversation window to a locally installed GGUF model. No network client, remote API, account, credential, automatic model download, external tools or research execution is included. The engine source is MIT licensed; the exact inspected upstream revision is 9c003a97ab34a69cc49243f9b02014038205ffae. VITQRA's original adapter is Apache-2.0. No upstream source, binary or weights are redistributed by this commit. Preserve all upstream licenses, copyright notices and transitive notices on installation and distribution.

## Build and review

1. Review the pinned llama.cpp source, native dependencies and licenses; obtain a clean checkout at the exact revision. The pin and public API were inspected, not a complete security audit. Audit native build outputs and their transitive dependencies, compare hashes with a trusted build manifest, and inspect for unexpected network backends before installation.
2. Build NativeAI with CMake and the Unity-compatible Android NDK: use the Android toolchain file, ANDROID_ABI=arm64-v8a, a compatible ANDROID_PLATFORM, LLAMA_SOURCE set to the reviewed checkout, and a Release build. Use a clean out-of-tree build. The CMake configuration disables server/tools, GPU, RPC, dynamic backend and optional download paths listed in the file. Verify the actual configuration and linked dependencies; do not assume a build is safe solely because options were requested.
3. Install the resulting libvitqra_llm.so and its reviewed runtime dependencies locally in the Unity Android ARM64 plugin directory. Do not commit binaries. For desktop, build the equivalent local library with the supported toolchain. No native compilation has been run in this environment.
4. Review a compact decoder-only instruction model independently, including original weights, quantization provenance, license, redistribution rights, source hashes, chat template support, language quality, memory and safety. No model is selected or approved here. Place a single reviewed GGUF file at Application.persistentDataPath/VITQRA/Models/model.gguf using an authorized local provisioning process. No installer is supplied. The Inspector requires the reviewed hash, SPDX license identifier, provenance and explicit enable setting on VitqraLocalAi attached to the spatial workspace. These declarations are not an independent license check. Never enter secrets in the fields. Keep the model file immutable while running.
5. Build and test on Quest. Without the component, configuration, file or matching plugin, no inference occurs. There is no fallback to a commercial service.

## Conversation

Send an explicit message using the existing keyboard or speech draft. The Send button becomes Cancel while generating. The model receives the language-specific system instruction, current message and at most one previous successful turn from the current process (up to 1000 UTF-16 units each). Restored archive history, notes, patient information and evidence files are not automatically passed to the model. Deleting history clears the inference context and cancels the current request. Minimizing the panel, pausing the application or destroying the workspace requests cancellation. Existing opt-in history policy and text/voice display remain. The model cannot browse, execute code or verify research; generated answers are untrusted and may be inaccurate. Do not enter sensitive data. The AI-generated output is displayed literally, not rich text.

Each request verifies the model header, size (maximum 2 GiB) and SHA-256 before invoking native inference, with a fixed private path. Hashing does not eliminate a local file replacement race or prove model safety. The bridge uses a reviewed-source ABI marker and one serialized job; CPU only, two threads, 2048-token context, 384-token generation, bounded output, no prompt logging by the wrapper, no automatic tool calls. Model and context are loaded per request; no persistent KV cache. The 120-second timeout requests cancellation, not a hard wall-clock guarantee. CPU abort and model-load callbacks are wired; actual cancellation and resource behavior must be tested. No guarantee of real-time response, sufficient headset memory, supported template or all ten languages. The system prompt requests the device language but cannot guarantee it. Existing speech synthesis may be unavailable or text-only.

## Research gate

No change to discovery, pagination, evidence acquisition or documentary review. Autonomous scientific research remains blocked. The assistant is conversational only. Verified challenge statements, rights, eligibility, deadlines, research execution and report generation require a separate pipeline and validation.

## Acceptance

Run VITQRA > Run Local AI Checks for managed smoke checks; not yet executed in Unity. Compile C++ and C#, audit the exact build and model, test with network disabled, check invalid hashes, missing library, memory/thermal limits, cancellation, localization, history deletion, malformed output, context limits and no outbound requests on physical Quest 3. No APK, model or runtime performance result is provided.

References:
- https://github.com/ggml-org/llama.cpp/tree/9c003a97ab34a69cc49243f9b02014038205ffae
- https://github.com/ggml-org/llama.cpp/blob/9c003a97ab34a69cc49243f9b02014038205ffae/LICENSE
- https://github.com/ggml-org/llama.cpp/blob/9c003a97ab34a69cc49243f9b02014038205ffae/docs/android.md
