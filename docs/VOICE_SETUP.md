# Local voice integration (v0.3.3)

The current C# bridge is not a speech engine. It defines an offline transcription contract and a microphone lifecycle for a future reviewed implementation. With no assigned engine, the existing Voice button reports that voice is unavailable and no microphone starts. No audio or native libraries are included.

## Candidate engine

Use sherpa-onnx or whisper.cpp only after auditing the pinned engine, transitive libraries and model licenses, including Android ARM64/IL2CPP support. Candidate sources: https://github.com/k2-fsa/sherpa-onnx and https://github.com/ggml-org/whisper.cpp. For TTS, review sherpa-onnx and separately licensed voices. A voice for every UI language is not yet established. Do not use Android system speech as an unverified offline fallback, cloud inference, paid APIs, or automatically downloaded media.

## Implementing the adapter

Implement VitqraOfflineSpeechEngine with a concrete local engine. IsReady must check an installed, verified model and supported locale. TranscribeAsync must use local PCM only, observe cancellation, return plain text, and avoid Unity API calls off the main thread. Assign the component to VitqraLocalVoice on the workspace before Configure. The bridge requests Android microphone permission only after an explicit button press, requires another press after permission grant, records at most 20 seconds, transcribes on stop, and inserts text into the draft for review. It does not auto-send. Closing the conversation, pausing or disabling the app cancels the operation. PCM arrays are cleared after processing; no recording file is created by the bridge. Engine implementations must also avoid logging or retaining audio. No native backend is integrated yet.

## Pending validation

- Install and audit a real native engine and model, including CPU/thermal/memory performance.
- Compile with Unity and IL2CPP and test permissions, capture, cancellation, empty audio, local-only operation, all supported languages and physical keyboard interaction.
- Add local TTS playback with volume, speed and profile preferences; preserve conversation text even if voice is unavailable.
- Verify audio is never sent to an external service. Do not bundle or publish unapproved voice or media assets.
