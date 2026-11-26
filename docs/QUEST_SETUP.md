# VITQRA 0.3.1 — Meta Quest spatial prototype

This source overlay adds four main world-space Unity UI panels, an on-demand avatar configuration panel and a temporary subtitles panel. It does not include a configured Unity project, an XR scene, an APK, a production avatar model, or a passthrough camera implementation. The Unity Mixed Reality template and its supported packages provide the XR rig and passthrough. Unity and headset testing are pending.

## Prepare Unity

1. Install a supported Unity 6 LTS editor (prefer 6.3 LTS when available in Unity Hub) with Android Build Support, SDK, NDK and OpenJDK.
2. Create a **Mixed Reality** template project in Unity Hub (not the desktop Universal 3D project). Use its XR Origin and configured passthrough example. Keep the template's compatible package versions; record the resolved Packages/manifest.json and packages-lock.json in your own test project. Do not replace the template rig with an ordinary camera.
3. Copy UnityStarter/Assets/VITQRA into Assets. The source uses Unity UI (com.unity.ugui) and UnityEngine.XR input. Install Unity UI from Package Manager if absent.
4. Select Android/Meta Quest build profile. Confirm XR Plug-in Management > Android > OpenXR, Meta Quest features, controller interaction profiles and passthrough settings. Resolve validation errors and select ARM64/IL2CPP. Use the template's existing passthrough setup (AR Camera Manager on the XR camera where required). Do not enable camera image capture or request camera access for this prototype.
5. In the template scene select the tracked Main Camera under XR Origin and choose **VITQRA > Add Spatial Workspace to Selected XR Camera**. Confirm that the serialized Tracking Space is the transform corresponding to the origin of controller tracking poses, normally the camera's Camera Offset parent. Keep the workspace root at world scale (1,1,1), outside the camera hierarchy.
6. Save the scene; include it in the build profile. Use application identifier com.blasllavero.vitqra and version 0.3.1. Build and Run on a developer-mode Quest 3. Do not publish a build until physical tests pass.

## Initial panel layout

- Left column: Research Challenges above My Notes.
- Right column: Smart Advice above AI Settings.
- The default panel centers are 0.85 m left/right of the viewing axis, 1.65 m forward, and 0.12 m / -0.42 m vertically relative to the head.
- Panels initially face the user's horizontal viewing position. Their 0.64 m width leaves an approximately 1.13 m horizontal central gap in the initial layout (geometric estimate; headset comfort and field of view must be tested).
- Each panel remains independently movable with the controller. The central gap is a default layout, not a restriction on user placement. A/X closes avatar configuration and resets panel positions relative to the user's current position and horizontal heading.

- Virtual Assistant is hidden at startup. Select Avatar settings on AI Settings to replace that panel temporarily with the avatar configuration. Next avatar cycles through four profiles; Close restores AI Settings. Subtitles appear independently above Smart Advice for assistant messages and are movable with grip. Close or the estimated reading duration hides them. The bust is a procedural placeholder, not a reviewed human model.

## Controller controls

- Aim the right controller at a panel (left controller is a fallback when right tracking is unavailable). The panel changes color on hover; a visible ray is not implemented.
- Hold grip on a panel and move the controller; release to leave the panel in world coordinates.
- Aim at the bottom action bar and press trigger to switch sample topics, review or advance a tip, open avatar settings, select an avatar, close the configuration or subtitles, reload the existing sample note, or toggle automatic research preference. AI Settings and Virtual Assistant each have two distinct action buttons.
- Press the active controller's primary button (A or X) to restore the initial layout.
- Tracking loss releases the grabbed panel. The layout is not saved across restarts and is not a persistent spatial anchor.
- These are local demonstration actions only. No live search, voice, AI calls, object recognition, browser or patient-data functionality are enabled. Hand confirmation is optional; see the assistant guide. Spatial labels are localized in ten languages; font coverage, RTL and visual layout require testing. Do not enter sensitive data.

## Physical acceptance checklist (not run)

- Android compilation succeeds without package or OpenXR validation errors.
- Passthrough is visible using the template and camera tracking behaves correctly.
- Four main panels appear after valid head tracking; a temporary subtitle may also appear, but the avatar stays hidden until Avatar settings is selected. Verify the four main panels, with Research Challenges and My Notes on the left and Smart Advice and AI Settings on the right. Confirm the central view is unobstructed by the panels at startup and after reset, the panels stay world-fixed when the user moves, face the user initially, and remain legible. Move each panel independently and verify that the other visible panels do not move.
- Both controllers can be tested independently, including controller tracking loss while grabbing. Grip moves the nearest hit panel; trigger acts only on the bottom bar; primary restores layout.
- No unexpected locomotion from template input bindings; disable conflicting sample interactions if needed. Verify comfortable distance, readable fonts and no obstruction.
- No AI calls, network services, medical data uploads or unexpected permissions are requested by VITQRA code.
- Test the desktop preview separately; verify English and Catalan strings and record any missing glyphs/RTL issues.

## Assistant and optional hand tracking

See [ASSISTANT_SETUP.md](ASSISTANT_SETUP.md) for enabling the gesture adapter and its acceptance checklist.

## Official setup references

- https://developers.meta.com/vr/documentation/unity/unity-xr-plugin/
- https://developers.meta.com/vr/documentation/unity/unity-and-openxr-compatibility/
- https://docs.unity.com/en-us/engine/6000.0/manual/xr/configuring-project-for/meta-quest-develop
- https://docs.unity.com/en-us/engine/6000.7/manual/xr/configuring-project-for/meta-quest-develop/packages
