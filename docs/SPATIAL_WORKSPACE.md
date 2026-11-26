# Persistent spatial workspace (v0.3.6)

The six spatial panels use stable English storage IDs independent of the device language. VITQRA saves their position, orientation and proportional scale automatically after a grip move ends, a resize action, conversation visibility change or workspace reset. A pending move is also saved on controller tracking loss, application pause or workspace destruction. The conversation minimized/open preference is restored. Avatar configuration is always closed at startup; its geometry is remembered, and the avatar itself remains hidden unless settings are explicitly opened.

## Coordinate system

Layout positions and rotations are relative to the initial head position and horizontal heading of the session (or the most recent A/X reset), not the head pose at exit. On a new launch, wait for tracked head pose, place the default panels, and apply the saved layout relative to the new starting pose. Panels remain world-fixed for the rest of the session. This is a portable personal arrangement, not a persistent room anchor or a room scan. Physical obstacles are not mapped; use the reset control if the restored layout is unsuitable.

Preferences use versioned JSON in local Unity PlayerPrefs. No network, paid APIs, absolute room coordinates, audio or images are used. This storage is not encrypted; it contains layout preferences only, separate from conversation history. Malformed or incompatible records fall back to defaults; individual entries outside the 0.5–4 m radius, 70–150% scale or valid rotation bounds are ignored on restoration. A user who walks far from the initial point and moves a panel may exceed that range. Such a panel restores to its default placement.

## Reset and visibility

A/X restores all default positions and 100% sizes relative to the current head pose, closes avatar settings, preserves the conversation open/minimized preference, and saves the reset layout. Resize controls remain hidden until a 0.5-second edge hover. Conversation auto-opens for new assistant messages during operation, but does not reopen solely to show a startup sample tip if previously minimized. Other panels maintain their existing visibility rules. Default layout leaves the center clear; a custom layout may not.

## Validation

Use Unity Editor > VITQRA > Run Spatial Layout Checks to check coordinate transforms, JSON round trips, scale and invalid data. The smoke test does not write preferences. This menu has not been run in the development environment.

Physical checklist: move and resize each panel; minimize the conversation; exit and reopen at another position and heading; verify relative layout and avatar hiding; test pause during a grip move, loss of tracking, language change, malformed preferences, reset and restart. Confirm resize hover behavior and that the restored layout does not obscure physical obstacles. Unity compilation and Quest 3 testing are pending.

The v0.7.0 optional avatar meeting editor exercise is isolated from normal startup and layout. No changes to the six spatial panels or avatar hiding preference. See [SYNTHETIC_AVATAR_MEETING.md](SYNTHETIC_AVATAR_MEETING.md).

The v0.7.1 synthetic telepresence projection is editor-only and does not change workspace geometry, saved layouts or normal avatar visibility. See [SYNTHETIC_TELEPRESENCE.md](SYNTHETIC_TELEPRESENCE.md).

The v0.8.0 object recognition exercise is editor-only, with no camera access, contextual runtime overlay or changes to the saved spatial arrangement. See [SYNTHETIC_OBJECT_RECOGNITION.md](SYNTHETIC_OBJECT_RECOGNITION.md).
