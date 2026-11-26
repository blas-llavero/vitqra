# Local neural speech (v0.3.4)

Source implementation: native Sherpa-ONNX C adapter, Unity C# interop, local microphone capture and optional TTS playback. No native binary, voice model, recording or APK is bundled. It has not been compiled or run in Unity or on Quest. The exact upstream C API inspected was sherpa-onnx commit 17d5f144035fb35e13b787445fd3a0100998aa5a; it is a source reference, not an approved dependency release. Use matching header/library versions, review licenses, and test all native symbols. Do not mix an older binary with a newer header.

## Native build and installation

1. Obtain reviewed Sherpa-ONNX source and matching Android arm64-v8a C API shared library and transitive libraries following upstream Android build instructions. Audit their licenses and checksums. No model or binary may be published before approval.
2. Build the VITQRA wrapper with the Unity-compatible Android NDK (API level 29 example; verify against actual project minimum SDK):
   ```sh
   cmake -S NativeSpeech -B build-speech \
     -DCMAKE_TOOLCHAIN_FILE="$ANDROID_NDK_HOME/build/cmake/android.toolchain.cmake" \
     -DANDROID_ABI=arm64-v8a -DANDROID_PLATFORM=android-29 \
     -DSHERPA_SOURCE=/path/to/reviewed/sherpa-onnx \
     -DSHERPA_LIBRARY=/path/to/matching/libsherpa-onnx-c-api.so
   cmake --build build-speech --config Release
   ```
3. In the generated Unity project, install libvitqra_speech.so and its matching native dependencies under Assets/Plugins/Android/arm64-v8a. Set Unity plugin import to Android ARM64; inspect ELF dependencies, alignment and licensing. Do not commit binaries to this repository. A Windows desktop build needs equivalent matching DLLs.
4. In the app-private Application.persistentDataPath/speech-models directory, provision reviewed model files by an authorized local installation process. No in-app installer or download is supplied. Add VitqraSherpaSpeechEngine to the XR workspace and populate Inspector relative file paths and SHA-256 values using a trusted, reviewed manifest. Configure multilingual Whisper encoder, decoder and tokens, and only languages verified for the model. The engine validates files asynchronously and rechecks their hashes before each inference. Do not modify files while running. The hash check verifies identity, not license or safety.
5. The existing VitqraLocalVoice component finds the engine on the same GameObject. It does not capture audio until the selected language is ready and the user explicitly presses Voice and grants permission. Press again to stop or wait for the existing time limit. Text goes into the draft; no automatic sending.
6. For TTS, configure a reviewed **lexicon-based VITS** model, lexicon and tokens per language/avatar profile, with a valid speaker ID. The inspected API version no longer uses the legacy eSpeak data_dir for phonemization. Models requiring external phonemization (including many Piper voices) are NOT supported by this wrapper yet. Do not assume coverage of all ten UI languages.
7. Add VitqraSpeechPlayback to the workspace and explicitly enable its Inspector setting for speech playback. Configure volume and speed. New assistant messages of up to 400 UTF-16 code units use the selected voice if ready. Longer messages, missing voice or failed synthesis stay text-only; no paid or cloud fallback. Text is displayed before playback, not word-synchronized. Runtime speech settings UI and long-text segmentation are future work.

## Privacy and resource constraints

No API requests, audio file output or automatic model downloads are implemented. Models load for each inference, with serialized native calls; readiness and hashing can be slow on headset. The native decoding call is not interruptible: cancellation discards its result and prevents playback, but computation can continue until it returns. Do not claim real-time cancellation of the backend. TTS playback stops on minimizing, microphone activation or app pause. No microphone starts if the ASR engine is missing or not ready. Text remains available if TTS is disabled or unavailable. Native engine logs and memory behavior require review. Do not enter sensitive patient or research data into this prototype.

## Validation (not run)

- Compile wrapper against matching audited header/library, inspect exported ABI symbols and transitive Android libraries.
- Test Unity editor import and IL2CPP Android ARM64 build; test native plugin loading on Quest 3.
- Verify known-good model fixtures, wrong hashes, missing plugin, missing language/voice, microphone permission denial, cancellation and no network access during inference.
- Test Catalan and the remaining nine languages independently with verified model rights, speech quality and speaker mappings. Benchmark RAM, latency and heat before selecting production models.
- Test speech rate and volume, long text behavior, headset pause, minimization and controller interactions. No release until physical tests pass.

References:
- https://github.com/k2-fsa/sherpa-onnx
- https://github.com/k2-fsa/sherpa-onnx/tree/17d5f144035fb35e13b787445fd3a0100998aa5a/c-api-examples
- https://k2-fsa.github.io/sherpa/onnx/c-api/html/index.html
