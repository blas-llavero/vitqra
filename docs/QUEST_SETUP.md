# VITQRA 0.2.0 — Meta Quest spatial prototype

This source overlay adds four world-space Unity UI panels. It does not include a configured Unity project, an XR scene, an APK, an avatar, or a passthrough camera implementation. The Unity Mixed Reality template and its supported packages provide the XR rig and passthrough. Unity and headset testing are pending.

## Prepare Unity

1. Install a supported Unity 6 LTS editor (prefer 6.3 LTS when available in Unity Hub) with Android Build Support, SDK, NDK and OpenJDK.
2. Create a **Mixed Reality** template project in Unity Hub (not the desktop Universal 3D project). Use its XR Origin and configured passthrough example. Keep the template's compatible package versions; record the resolved Packages/manifest.json and packages-lock.json in your own test project. Do not replace the template rig with an ordinary camera.
3. Copy UnityStarter/Assets/VITQRA into Assets. The source uses Unity UI (com.unity.ugui) and UnityEngine.XR input. Install Unity UI from Package Manager if absent.
4. Select Android/Meta Quest build profile. Confirm XR Plug-in Management > Android > OpenXR, Meta Quest features, controller interaction profiles and passthrough settings. Resolve validation errors and select ARM64/IL2CPP. Use the template's existing passthrough setup (AR Camera Manager on the XR camera where required). Do not enable camera image capture or request camera access for this prototype.
5. In the template scene select the tracked Main Camera under XR Origin and choose **VITQRA > Add Spatial Workspace to Selected XR Camera**. Confirm that the serialized Tracking Space is the transform corresponding to the origin of controller tracking poses, normally the camera's Camera Offset parent. Keep the workspace root at world scale (1,1,1), outside the camera hierarchy.
6. Save the scene; include it in the build profile. Use application identifier com.blasllavero.vitqra and version 0.2.0. Build and Run on a developer-mode Quest 3. Do not publish a build until physical tests pass.

## Controller controls

- Aim the right controller at a panel (left controller is a fallback when right tracking is unavailable). The panel changes color on hover; a visible ray is not implemented.
- Hold grip on a panel and move the controller; release to leave the panel in world coordinates.
- Aim at the bottom action bar and press trigger to switch sample topics/tips, reload the existing sample note, or toggle automatic research preference.
- Press the active controller's primary button (A or X) to restore the initial layout.
- Tracking loss releases the grabbed panel. The layout is not saved across restarts and is not a persistent spatial anchor.
- These are local demonstration actions only. No live search, voice, AI calls, object recognition, browser, patient-data functionality or hand gestures are enabled. New spatial labels are partially English; existing translated labels are reused. Do not enter sensitive data.

## Physical acceptance checklist (not run)

- Android compilation succeeds without package or OpenXR validation errors.
- Passthrough is visible using the template and camera tracking behaves correctly.
- All four panels appear after valid head tracking, stay world-fixed when the user moves, face the user initially, and remain legible.
- Both controllers can be tested independently, including controller tracking loss while grabbing. Grip moves the nearest hit panel; trigger acts only on the bottom bar; primary restores layout.
- No unexpected locomotion from template input bindings; disable conflicting sample interactions if needed. Verify comfortable distance, readable fonts and no obstruction.
- No AI calls, network services, medical data uploads or unexpected permissions are requested by VITQRA code.
- Test the desktop preview separately; verify English and Catalan strings and record any missing glyphs/RTL issues.

## Official setup references

- https://developers.meta.com/vr/documentation/unity/unity-xr-plugin/
- https://developers.meta.com/vr/documentation/unity/unity-and-openxr-compatibility/
- https://docs.unity.com/en-us/engine/6000.0/manual/xr/configuring-project-for/meta-quest-develop
- https://docs.unity.com/en-us/engine/6000.7/manual/xr/configuring-project-for/meta-quest-develop/packages
