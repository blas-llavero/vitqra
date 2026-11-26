# Conversation prototype (v0.3.2)

The large Conversation panel replaces temporary subtitles. It opens automatically on startup or when an assistant message or sample advice is presented. It occupies the upper right slot instead of Smart Advice, leaving the central view clear and AI Settings accessible below. Its initial center is 1.00 m to the right and 1.65 m forward, 0.34 m above eye level; width 0.86 m and height 0.90 m. These are provisional spatial dimensions. Grip moves it independently. A/X resets positions while respecting whether it was minimized. Minimize restores Smart Advice; Open conversation reopens it. The avatar remains hidden unless its settings are opened.

## Controls

- Older / Newer browse message fragments in pages of four. New messages jump to the latest page. User and assistant messages have different alignment and color. Sample tips are labeled as such, not AI-generated replies.
- Select the composer or Write to invoke Unity TouchScreenKeyboard if supported. On a desktop/editor without it, a temporary IMGUI text editor is provided. Done copies the draft; Send adds it to the session history. Android keyboard availability and appearance on Quest need physical validation.
- Voice is an integration point only. When no speech adapter subscribes, the UI states that voice is not connected. ApplyVoiceTranscript(text) can insert a transcript into the draft for review, without sending it automatically.
- MessageSubmitted is an event on VitqraConversationPanel for a future AI adapter. No subscriber is included; the UI explicitly indicates that AI is not connected. AddAssistantMessage(message) is the workspace's main-thread entry point for a future reply. No real AI replies are generated.
- History is bounded to the last 200 messages, in memory only, not written to disk or sent externally by this prototype. Drafts are limited to 1000 UTF-16 code units. Do not enter patient data, credentials or confidential information. Restarting clears the history. A future persistent history requires deletion controls and a privacy review.
- Text is localized in ten languages; font glyphs, shaping, wrapping and readability remain unverified. No audio, media or models included.

## Acceptance checklist (not run)

- Compile in Unity, build on Quest 3, test keyboard availability, cancellation, typing and sending; test Bluetooth keyboard with the platform keyboard if supported.
- Verify visible right-side conversation and AI Settings do not overlap from the user's viewpoint; verify the center is clear and adjust the provisional layout after testing.
- Move the conversation independently; minimize/reopen without losing messages; reset positions and verify the avatar remains hidden.
- Test history paging, empty drafts, long text, Unicode and right-to-left layout, the message limit and restart behavior.
- Verify voice shows an unavailable state without starting a microphone; verify no fabricated AI replies or paid API requests.
- Verify speech/AI adapters, when implemented, respect consent, cost controls, cancellation and local-only privacy preferences.
